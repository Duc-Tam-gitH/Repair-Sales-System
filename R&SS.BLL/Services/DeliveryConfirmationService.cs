using FluentValidation;
using FluentValidation.Results;
using R_SS.BLL.DTOs.Delivery;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class DeliveryConfirmationService : IDeliveryConfirmationService
{
    private const string PendingConfirmationStatus = "Pending Delivery Confirmation";
    private const string DeliveredStatus = "Delivered";
    private const string UnderRepairStatus = "Under Repair";
    private const string PendingManualDeliveryStatus = "Pending Manual Delivery";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly IOtpGenerator _otpGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<ConfirmDeliveryRequest> _confirmValidator;
    private readonly IValidator<RejectDeliveryRequest> _rejectValidator;
    private readonly IValidator<ResendDeliveryOtpRequest> _resendValidator;

    public DeliveryConfirmationService(
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        IOtpGenerator otpGenerator,
        IPasswordHasher passwordHasher,
        IValidator<ConfirmDeliveryRequest> confirmValidator,
        IValidator<RejectDeliveryRequest> rejectValidator,
        IValidator<ResendDeliveryOtpRequest> resendValidator)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _otpGenerator = otpGenerator;
        _passwordHasher = passwordHasher;
        _confirmValidator = confirmValidator;
        _rejectValidator = rejectValidator;
        _resendValidator = resendValidator;
    }

    public async Task<IReadOnlyCollection<PendingDeliveryTicketResponse>> GetPendingAsync(int customerId)
    {
        if (customerId <= 0)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(customerId), "CustomerId must be greater than 0.") });
        }

        var tickets = await _unitOfWork.RepairOrders.GetSubmittedByCustomerIdAsync(customerId, includeCanceled: false);
        return tickets
            .Where(ticket => ticket.Status.Equals(PendingConfirmationStatus, StringComparison.OrdinalIgnoreCase))
            .Select(ticket => new PendingDeliveryTicketResponse
            {
                RepairOrderId = ticket.RepairOrderId,
                TicketCode = ticket.RepairOrderCode,
                DeviceType = ticket.DeviceType,
                Brand = ticket.Brand,
                DeadlineUtc = ticket.DeliveryConfirmationDeadlineUtc
            })
            .ToArray();
    }

    public async Task<DeliveryConfirmationResponse> ConfirmAsync(ConfirmDeliveryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _confirmValidator.ValidateAsync(request));

        var ticket = await GetCustomerPendingTicketAsync(request.RepairOrderId, request.CustomerId);
        EnsureNotLocked(ticket);
        EnsureDeadlineIsValid(ticket);

        if (string.IsNullOrWhiteSpace(ticket.DeliveryOtpHash) ||
            ticket.DeliveryOtpExpiresAtUtc < DateTime.UtcNow ||
            !_passwordHasher.Verify(request.OtpCode, ticket.DeliveryOtpHash))
        {
            ticket.DeliveryOtpAttemptCount++;
            if (ticket.DeliveryOtpAttemptCount >= 5)
            {
                ticket.DeliveryConfirmationLockedUntilUtc = DateTime.UtcNow.AddMinutes(15);
            }

            _unitOfWork.RepairOrders.Update(ticket);
            await _unitOfWork.SaveChangesAsync();
            throw new UnauthorizedException("OTP code is invalid or expired.");
        }

        ticket.Status = DeliveredStatus;
        ticket.DeliveryConfirmedAtUtc = DateTime.UtcNow;
        ticket.DeliveryConfirmationMethod = "OTP";
        ticket.DeliveryConfirmationIpAddress = request.IpAddress;
        ticket.DeliveryOtpHash = null;
        ticket.DeliveryOtpAttemptCount = 0;
        ticket.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.RepairOrders.Update(ticket);
        await AddStatusHistoryAsync(ticket, null, DeliveredStatus, "Customer confirmed device delivery by OTP.");
        await _unitOfWork.SaveChangesAsync();

        return Map(ticket, "Device delivery confirmed successfully.");
    }

    public async Task<DeliveryConfirmationResponse> RejectAsync(RejectDeliveryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _rejectValidator.ValidateAsync(request));

        var ticket = await GetCustomerPendingTicketAsync(request.RepairOrderId, request.CustomerId);
        EnsureDeadlineIsValid(ticket);

        ticket.DeliveryRejectionCount++;
        ticket.Status = ticket.DeliveryRejectionCount > 2 ? PendingManualDeliveryStatus : UnderRepairStatus;
        ticket.Notes = string.IsNullOrWhiteSpace(ticket.Notes)
            ? $"Delivery rejected: {request.Reason.Trim()}"
            : $"{ticket.Notes}; Delivery rejected: {request.Reason.Trim()}";
        ticket.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.RepairOrders.Update(ticket);
        await AddStatusHistoryAsync(ticket, null, ticket.Status, request.Reason.Trim());
        await _unitOfWork.SaveChangesAsync();

        return Map(ticket, ticket.Status == PendingManualDeliveryStatus
            ? "Delivery rejection recorded. Manual processing is required."
            : "Delivery rejection recorded. Ticket returned to repair processing.");
    }

    public async Task<DeliveryConfirmationResponse> ResendOtpAsync(ResendDeliveryOtpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _resendValidator.ValidateAsync(request));

        var ticket = await GetCustomerPendingTicketAsync(request.RepairOrderId, request.CustomerId);
        EnsureNotLocked(ticket);
        EnsureDeadlineIsValid(ticket);

        if (ticket.DeliveryOtpResendCount >= 3)
        {
            throw new InvalidOperationException("Maximum OTP resend limit reached.");
        }

        if (string.IsNullOrWhiteSpace(ticket.Customer?.Email))
        {
            throw new InvalidOperationException("Customer email is required to resend OTP.");
        }

        var otp = _otpGenerator.Generate(6);
        ticket.DeliveryOtpHash = _passwordHasher.Hash(otp);
        ticket.DeliveryOtpExpiresAtUtc = DateTime.UtcNow.AddMinutes(15);
        ticket.DeliveryOtpSentAtUtc = DateTime.UtcNow;
        ticket.DeliveryOtpSentToEmail = ticket.Customer.Email;
        ticket.DeliveryOtpResendCount++;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _emailSender.SendDeliveryConfirmationOtpAsync(ticket.Customer.Email, ticket.Customer.FullName, ticket.RepairOrderCode, otp);
        _unitOfWork.RepairOrders.Update(ticket);
        await _unitOfWork.SaveChangesAsync();

        return Map(ticket, "Delivery confirmation OTP resent successfully.");
    }

    private async Task<RepairOrder> GetCustomerPendingTicketAsync(int repairOrderId, int customerId)
    {
        var ticket = await _unitOfWork.RepairOrders.GetWithDetailsAsync(repairOrderId);
        if (ticket is null || ticket.CustomerId != customerId)
        {
            throw new NotFoundException("Technical ticket not found.");
        }

        if (!ticket.Status.Equals(PendingConfirmationStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Technical ticket is not pending delivery confirmation.");
        }

        return ticket;
    }

    private static void EnsureNotLocked(RepairOrder ticket)
    {
        if (ticket.DeliveryConfirmationLockedUntilUtc > DateTime.UtcNow)
        {
            throw new UnauthorizedException("Delivery confirmation is temporarily locked.");
        }
    }

    private static void EnsureDeadlineIsValid(RepairOrder ticket)
    {
        if (ticket.DeliveryConfirmationDeadlineUtc < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Delivery confirmation deadline has expired.");
        }
    }

    private async Task AddStatusHistoryAsync(RepairOrder ticket, int? actorUserId, string status, string notes)
    {
        await _unitOfWork.RepairOrderStatusHistories.AddAsync(new RepairOrderStatusHistory
        {
            RepairOrderId = ticket.RepairOrderId,
            RepairOrder = ticket,
            UpdatedByUserId = actorUserId ?? 0,
            Status = status,
            Notes = notes,
            UpdatedAtUtc = DateTime.UtcNow
        });
    }

    private static DeliveryConfirmationResponse Map(RepairOrder ticket, string message) => new()
    {
        RepairOrderId = ticket.RepairOrderId,
        TicketCode = ticket.RepairOrderCode,
        Status = ticket.Status,
        DeliveryConfirmedAtUtc = ticket.DeliveryConfirmedAtUtc,
        Message = message
    };

    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
