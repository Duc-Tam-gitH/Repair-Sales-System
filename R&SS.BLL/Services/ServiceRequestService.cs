using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.DTOs.ServiceRequest;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class ServiceRequestService : IServiceRequestService
{
    private const string PendingReceptionStatus = "Pending Reception";
    private const string CanceledStatus = "Canceled";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<SendServiceRequestRequest> _sendValidator;
    private readonly IValidator<CancelServiceRequestRequest> _cancelValidator;
    private readonly ILogger<ServiceRequestService> _logger;

    public ServiceRequestService(
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        IValidator<SendServiceRequestRequest> sendValidator,
        IValidator<CancelServiceRequestRequest> cancelValidator,
        ILogger<ServiceRequestService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _sendValidator = sendValidator;
        _cancelValidator = cancelValidator;
        _logger = logger;
    }

    public async Task<ServiceRequestResponse> SendAsync(SendServiceRequestRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _sendValidator.ValidateAsync(request));

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
        if (customer is null || !customer.IsActive)
        {
            throw new NotFoundException("Customer not found.");
        }

        var serviceRequest = new ServiceRequest
        {
            CustomerId = customer.CustomerId,
            Customer = customer,
            RequestCode = await CreateUniqueCodeAsync(),
            ServiceType = request.ServiceType.Trim(),
            DeviceType = request.DeviceType.Trim(),
            Brand = request.Brand.Trim(),
            DeviceModel = Normalize(request.DeviceModel),
            Description = request.Description.Trim(),
            Status = PendingReceptionStatus,
            ImageUrls = string.Join(";", request.Images.Select(image => image.FileName.Trim())),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        try
        {
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                await _emailSender.SendServiceRequestReceivedAsync(customer.Email, customer.FullName, serviceRequest.RequestCode);
            }

            await _emailSender.SendInternalServiceRequestNotificationAsync(serviceRequest.RequestCode);
        }
        catch (Exception exception)
        {
            serviceRequest.NeedsManualProcessing = true;
            _logger.LogWarning(exception, "Notification failed for service request {RequestCode}.", serviceRequest.RequestCode);
        }

        await _unitOfWork.ServiceRequests.AddAsync(serviceRequest);
        await _unitOfWork.SaveChangesAsync();

        return Map(serviceRequest, "Service request sent successfully.");
    }

    public async Task<ServiceRequestResponse> CancelAsync(CancelServiceRequestRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _cancelValidator.ValidateAsync(request));

        var serviceRequest = await _unitOfWork.ServiceRequests.GetByIdAsync(request.ServiceRequestId);
        if (serviceRequest is null || serviceRequest.CustomerId != request.CustomerId)
        {
            throw new NotFoundException("Service request not found.");
        }

        if (!serviceRequest.Status.Equals(PendingReceptionStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only pending reception service requests can be canceled.");
        }

        serviceRequest.Status = CanceledStatus;
        serviceRequest.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.ServiceRequests.Update(serviceRequest);
        await _unitOfWork.SaveChangesAsync();

        return Map(serviceRequest, "Service request canceled successfully.");
    }

    public async Task<IReadOnlyCollection<ServiceRequestResponse>> GetByCustomerAsync(int customerId)
    {
        if (customerId <= 0)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(customerId), "CustomerId must be greater than 0.") });
        }

        var requests = await _unitOfWork.ServiceRequests.GetByCustomerIdAsync(customerId);
        return requests.Select(request => Map(request, string.Empty)).ToArray();
    }

    private async Task<string> CreateUniqueCodeAsync()
    {
        string code;
        do
        {
            code = $"SR-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }
        while (await _unitOfWork.ServiceRequests.ExistsCodeAsync(code));

        return code;
    }

    private static ServiceRequestResponse Map(ServiceRequest request, string message) => new()
    {
        ServiceRequestId = request.ServiceRequestId,
        RequestCode = request.RequestCode,
        CustomerId = request.CustomerId,
        ServiceType = request.ServiceType,
        DeviceType = request.DeviceType,
        Brand = request.Brand,
        DeviceModel = request.DeviceModel,
        Description = request.Description,
        Status = request.Status,
        NeedsManualProcessing = request.NeedsManualProcessing,
        ImageFileNames = string.IsNullOrWhiteSpace(request.ImageUrls)
            ? Array.Empty<string>()
            : request.ImageUrls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        Message = message
    };

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
