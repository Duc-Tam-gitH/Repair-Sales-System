using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Customer;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CustomerSearchRequest> _searchValidator;
    private readonly IValidator<CreateCustomerRequest> _createValidator;
    private readonly IValidator<UpdateCustomerRequest> _updateValidator;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CustomerSearchRequest> searchValidator,
        IValidator<CreateCustomerRequest> createValidator,
        IValidator<UpdateCustomerRequest> updateValidator,
        ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _searchValidator = searchValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>
    /// Searches or lists customers for an authorized staff user.
    /// </summary>
    public async Task<CustomerListResponse> SearchAsync(CustomerSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _searchValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);
        EnsureCustomerManagerRole(request.ActorRole);

        var customers = await _unitOfWork.Customers.SearchAsync(request.Keyword);
        var mappedCustomers = _mapper.Map<IReadOnlyCollection<CustomerResponse>>(customers);

        return new CustomerListResponse
        {
            Customers = mappedCustomers,
            Message = mappedCustomers.Count == 0 ? "No customers found." : "Customers retrieved successfully."
        };
    }

    /// <summary>
    /// Gets customer details for an authorized staff user.
    /// </summary>
    public async Task<CustomerResponse> GetByIdAsync(int customerId, string actorRole)
    {
        if (customerId <= 0)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(customerId), "Customer id must be greater than 0.") });
        }

        EnsureCustomerManagerRole(actorRole);

        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer is null)
        {
            throw new NotFoundException("Customer not found.");
        }

        var response = _mapper.Map<CustomerResponse>(customer);
        response.Message = "Customer retrieved successfully.";
        return response;
    }

    /// <summary>
    /// Creates a customer profile.
    /// </summary>
    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _createValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);
        EnsureCustomerManagerRole(request.ActorRole);

        await EnsureCustomerUniqueAsync(request.CustomerCode, request.Phone, request.Email, null);

        var customer = new Customer
        {
            CustomerCode = request.CustomerCode.Trim(),
            FullName = request.FullName.Trim(),
            Phone = NormalizeOptional(request.Phone),
            Email = NormalizeOptional(request.Email),
            Address = NormalizeOptional(request.Address),
            Notes = NormalizeOptional(request.Notes),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("User {UserId} created customer {CustomerCode}.", request.ActorUserId, customer.CustomerCode);

        var response = _mapper.Map<CustomerResponse>(customer);
        response.Message = "Customer added successfully.";
        return response;
    }

    /// <summary>
    /// Activates or deactivates a customer and records update history.
    /// </summary>
    public async Task<CustomerResponse> UpdateStatusAsync(UpdateCustomerStatusRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CustomerId <= 0)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.CustomerId), "Customer id must be greater than 0.") });
        }

        EnsureCustomerManagerRole(request.ActorRole);

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
        if (customer is null)
        {
            throw new NotFoundException("Customer not found.");
        }

        var oldStatus = customer.IsActive;
        customer.IsActive = request.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.CustomerUpdateHistories.AddAsync(new CustomerUpdateHistory
        {
            Customer = customer,
            CustomerId = customer.CustomerId,
            UpdatedByUserId = request.ActorUserId,
            UpdatedAtUtc = DateTime.UtcNow,
            ChangedContent = $"IsActive: '{oldStatus}' -> '{request.IsActive}'" +
                (string.IsNullOrWhiteSpace(request.Reason) ? string.Empty : $"; Reason: {request.Reason.Trim()}")
        });
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("User {UserId} updated customer {CustomerId} status.", request.ActorUserId, customer.CustomerId);

        var response = _mapper.Map<CustomerResponse>(customer);
        response.Message = "Customer status updated successfully.";
        return response;
    }

    /// <summary>
    /// Updates customer information and records update history.
    /// </summary>
    public async Task<CustomerResponse> UpdateAsync(UpdateCustomerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _updateValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);
        EnsureCustomerManagerRole(request.ActorRole);

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
        if (customer is null)
        {
            throw new NotFoundException("Customer not found.");
        }

        await EnsureCustomerUniqueAsync(customer.CustomerCode, request.Phone, request.Email, customer.CustomerId);
        var changedContent = BuildChangedContent(customer, request);

        customer.FullName = request.FullName.Trim();
        customer.Phone = NormalizeOptional(request.Phone);
        customer.Email = NormalizeOptional(request.Email);
        customer.Address = NormalizeOptional(request.Address);
        customer.Notes = NormalizeOptional(request.Notes);
        customer.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.CustomerUpdateHistories.AddAsync(new CustomerUpdateHistory
        {
            Customer = customer,
            CustomerId = customer.CustomerId,
            UpdatedByUserId = request.ActorUserId,
            UpdatedAtUtc = DateTime.UtcNow,
            ChangedContent = changedContent
        });
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("User {UserId} updated customer {CustomerId}.", request.ActorUserId, customer.CustomerId);

        var response = _mapper.Map<CustomerResponse>(customer);
        response.Message = "Customer updated successfully.";
        return response;
    }

    private async Task EnsureCustomerUniqueAsync(string customerCode, string? phone, string? email, int? excludedCustomerId)
    {
        if (await _unitOfWork.Customers.ExistsCodeAsync(customerCode, excludedCustomerId))
        {
            throw new InvalidOperationException("Customer code already exists.");
        }

        if (!string.IsNullOrWhiteSpace(phone) && await _unitOfWork.Customers.ExistsPhoneAsync(phone.Trim(), excludedCustomerId))
        {
            throw new InvalidOperationException("Phone number already exists.");
        }

        if (!string.IsNullOrWhiteSpace(email) && await _unitOfWork.Customers.ExistsEmailAsync(email.Trim(), excludedCustomerId))
        {
            throw new InvalidOperationException("Email already exists.");
        }
    }

    private static string BuildChangedContent(Customer customer, UpdateCustomerRequest request)
    {
        var changes = new List<string>();
        AddChange(changes, nameof(Customer.FullName), customer.FullName, request.FullName);
        AddChange(changes, nameof(Customer.Phone), customer.Phone, request.Phone);
        AddChange(changes, nameof(Customer.Email), customer.Email, request.Email);
        AddChange(changes, nameof(Customer.Address), customer.Address, request.Address);
        AddChange(changes, nameof(Customer.Notes), customer.Notes, request.Notes);
        return changes.Count == 0 ? "No field changes." : string.Join("; ", changes);
    }

    private static void AddChange(List<string> changes, string fieldName, string? oldValue, string? newValue)
    {
        var normalizedOldValue = NormalizeOptional(oldValue) ?? string.Empty;
        var normalizedNewValue = NormalizeOptional(newValue) ?? string.Empty;

        if (!string.Equals(normalizedOldValue, normalizedNewValue, StringComparison.Ordinal))
        {
            changes.Add($"{fieldName}: '{normalizedOldValue}' -> '{normalizedNewValue}'");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void EnsureCustomerManagerRole(string role)
    {
        if (!string.Equals(role, RoleConstants.Receptionist, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role, RoleConstants.Manager, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Receptionist or Manager roles can access customer management.");
        }
    }

    private static void ThrowIfInvalid(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
