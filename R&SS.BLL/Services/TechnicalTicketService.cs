using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.TechnicalOrder;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class TechnicalTicketService : ITechnicalTicketService
{
    private const string PendingReceptionStatus = "Pending Reception";
    private const string ReceivedStatus = "Received";
    private const string HandedOverToTechnicianStatus = "Handed Over to Technician";
    private const string PendingDeliveryConfirmationStatus = "Pending Delivery Confirmation";
    private const string PendingManualDeliveryStatus = "Pending Manual Delivery";
    private const string PendingDeliveryStatus = "Pending Delivery";
    private const string DeliveredStatus = "Delivered";
    private const string CompletedStatus = "Completed";
    private const string CancelledStatus = "Cancelled";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly IOtpGenerator _otpGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<CreateTechnicalTicketRequest> _createValidator;
    private readonly IValidator<ViewTechnicalTicketsRequest> _viewValidator;
    private readonly IValidator<AssignTechnicianRequest> _assignValidator;
    private readonly IValidator<UpdateTechnicalTicketRequest> _updateValidator;
    private readonly IValidator<ConfirmRepairPaymentRequest> _paymentValidator;
    private readonly IValidator<CancelTechnicalTicketRequest> _cancelValidator;
    private readonly ILogger<TechnicalTicketService> _logger;

    public TechnicalTicketService(
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        IOtpGenerator otpGenerator,
        IPasswordHasher passwordHasher,
        IValidator<CreateTechnicalTicketRequest> createValidator,
        IValidator<ViewTechnicalTicketsRequest> viewValidator,
        IValidator<AssignTechnicianRequest> assignValidator,
        IValidator<UpdateTechnicalTicketRequest> updateValidator,
        IValidator<ConfirmRepairPaymentRequest> paymentValidator,
        IValidator<CancelTechnicalTicketRequest> cancelValidator,
        ILogger<TechnicalTicketService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _otpGenerator = otpGenerator;
        _passwordHasher = passwordHasher;
        _createValidator = createValidator;
        _viewValidator = viewValidator;
        _assignValidator = assignValidator;
        _updateValidator = updateValidator;
        _paymentValidator = paymentValidator;
        _cancelValidator = cancelValidator;
        _logger = logger;
    }

    public async Task<TechnicalTicketResponse> CreateAsync(CreateTechnicalTicketRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _createValidator.ValidateAsync(request));
        EnsureReceptionRole(request.ActorRole);

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
        if (customer is null)
        {
            throw new NotFoundException("Customer not found.");
        }

        var technician = await GetAssignableTechnicianAsync(request.AssignedTechnicianId);
        ServiceRequest? sourceRequest = null;
        if (request.SourceServiceRequestId.HasValue)
        {
            sourceRequest = await _unitOfWork.ServiceRequests.GetByIdAsync(request.SourceServiceRequestId.Value);
            if (sourceRequest is null || !sourceRequest.Status.Equals(PendingReceptionStatus, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Service request is no longer pending reception.");
            }
        }

        await _unitOfWork.BeginTransactionAsync();
        RepairOrder ticket;
        try
        {
            customer.Email = request.CustomerEmail.Trim();
            customer.Phone = request.CustomerPhone.Trim();
            _unitOfWork.Customers.Update(customer);

            ticket = new RepairOrder
            {
                RepairOrderCode = CreateTicketCode(),
                CustomerId = customer.CustomerId,
                Customer = customer,
                ReceivedByUserId = request.ReceivedByUserId,
                AssignedTechnicianId = technician.UserId,
                AssignedTechnician = technician,
                DeviceType = request.DeviceType.Trim(),
                Brand = request.Brand.Trim(),
                DeviceName = Normalize(request.DeviceName),
                DeviceModel = Normalize(request.DeviceModel),
                SerialNumber = Normalize(request.SerialNumber),
                RequestType = request.RequestType.Trim(),
                IssueDescription = request.IssueDescription.Trim(),
                DeviceCondition = request.DeviceCondition.Trim(),
                Status = HandedOverToTechnicianStatus,
                Notes = Normalize(request.Notes),
                ReceivedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            ticket.StatusHistories.Add(new RepairOrderStatusHistory
            {
                RepairOrder = ticket,
                UpdatedByUserId = request.ReceivedByUserId,
                Status = ReceivedStatus,
                Notes = "Ticket received by receptionist.",
                UpdatedAtUtc = DateTime.UtcNow
            });

            ticket.StatusHistories.Add(new RepairOrderStatusHistory
            {
                RepairOrder = ticket,
                UpdatedByUserId = request.ReceivedByUserId,
                Status = HandedOverToTechnicianStatus,
                Notes = $"Work handed over to technician {technician.FullName}.",
                UpdatedAtUtc = DateTime.UtcNow
            });

            ticket.AssignmentHistories.Add(new TechnicianAssignmentHistory
            {
                RepairOrder = ticket,
                AssignedByUserId = request.ReceivedByUserId,
                AssignedTechnicianId = technician.UserId,
                Notes = "Assigned and handed over during ticket creation.",
                AssignedAtUtc = DateTime.UtcNow
            });

            if (sourceRequest is not null)
            {
                sourceRequest.Status = ReceivedStatus;
                sourceRequest.RepairOrder = ticket;
                sourceRequest.UpdatedAtUtc = DateTime.UtcNow;
                _unitOfWork.ServiceRequests.Update(sourceRequest);
            }

            await _unitOfWork.RepairOrders.AddAsync(ticket);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw new InvalidOperationException("Failed to assign technician. Please try again.");
        }

        await _emailSender.SendTechnicalTicketCreatedAsync(customer.Email!, customer.FullName, ticket.RepairOrderCode);
        _logger.LogInformation("Created technical ticket {TicketCode}.", ticket.RepairOrderCode);

        var response = MapTicket(ticket, RoleConstants.Receptionist, customer.CustomerId);
        response.Message = "Technical ticket created successfully.";
        return response;
    }

    private async Task<User> GetAssignableTechnicianAsync(int technicianId)
    {
        var technicians = await _unitOfWork.Users.GetTechniciansAsync();
        var technician = technicians.FirstOrDefault(user => user.UserId == technicianId);
        if (technician is null)
        {
            throw new InvalidOperationException("Failed to assign technician. Please try again.");
        }

        if (!technician.IsActive || technician.AccountLockedUntilUtc > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Failed to assign technician. Please try again.");
        }

        return technician;
    }

    public async Task<TechnicalTicketListResponse> GetTicketsAsync(ViewTechnicalTicketsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _viewValidator.ValidateAsync(request));

        var tickets = await _unitOfWork.RepairOrders.GetVisibleTicketsAsync(request.ActorRole, request.ActorUserId, request.CustomerId);
        var mapped = tickets.Select(ticket => MapTicket(ticket, request.ActorRole, request.CustomerId)).ToArray();
        return new TechnicalTicketListResponse
        {
            Tickets = mapped,
            Message = mapped.Length == 0 ? "No technical tickets found." : "Technical tickets retrieved successfully."
        };
    }

    public async Task<TechnicalTicketResponse> GetDetailsAsync(int repairOrderId, ViewTechnicalTicketsRequest viewer)
    {
        ValidateTicketId(repairOrderId);
        ThrowIfInvalid(await _viewValidator.ValidateAsync(viewer));
        var ticket = await GetAuthorizedTicketAsync(repairOrderId, viewer);
        var response = MapTicket(ticket, viewer.ActorRole, viewer.CustomerId);
        response.Message = "Technical ticket information retrieved successfully.";
        return response;
    }

    public async Task<TechnicalTicketProgressResponse> TrackProgressAsync(int repairOrderId, ViewTechnicalTicketsRequest viewer)
    {
        var ticket = await GetAuthorizedTicketAsync(repairOrderId, viewer);
        return new TechnicalTicketProgressResponse
        {
            Ticket = MapTicket(ticket, viewer.ActorRole, viewer.CustomerId),
            History = ticket.StatusHistories
                .OrderBy(history => history.UpdatedAtUtc)
                .Select(history => new ProgressHistoryResponse
                {
                    Status = history.Status,
                    Notes = history.Notes,
                    UpdatedAtUtc = history.UpdatedAtUtc
                })
                .ToArray(),
            Message = "Technical ticket progress retrieved successfully."
        };
    }

    public async Task<TechnicianListResponse> GetTechniciansAsync(string actorRole)
    {
        EnsureReceptionRole(actorRole);
        var technicians = await _unitOfWork.Users.GetTechniciansAsync();
        var workloads = new List<TechnicianWorkloadResponse>();
        foreach (var technician in technicians)
        {
            workloads.Add(new TechnicianWorkloadResponse
            {
                TechnicianId = technician.UserId,
                FullName = technician.FullName,
                Specialization = technician.EmployeeProfile?.Specialization,
                WorkStatus = technician.AccountLockedUntilUtc > DateTime.UtcNow
                    ? "Temporarily Locked"
                    : technician.EmployeeProfile?.WorkStatus ?? "Working",
                ActiveTicketCount = await _unitOfWork.RepairOrders.CountActiveByTechnicianAsync(technician.UserId)
            });
        }

        return new TechnicianListResponse
        {
            Technicians = workloads,
            Message = workloads.Count == 0 ? "No technicians found." : "Technicians retrieved successfully."
        };
    }

    public async Task<TechnicalTicketResponse> AssignTechnicianAsync(AssignTechnicianRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _assignValidator.ValidateAsync(request));
        EnsureReceptionRole(request.ActorRole);

        var ticket = await _unitOfWork.RepairOrders.GetWithDetailsAsync(request.RepairOrderId);
        if (ticket is null || IsClosed(ticket.Status))
        {
            throw new InvalidOperationException("Technical ticket does not exist or has been completed.");
        }

        var technician = await _unitOfWork.Users.GetByIdAsync(request.TechnicianId);
        if (technician is null)
        {
            throw new NotFoundException("Technician not found.");
        }

        if (!technician.IsActive || technician.AccountLockedUntilUtc > DateTime.UtcNow)
        {
            throw new UnauthorizedException("Technician cannot be assigned.");
        }

        var previousTechnicianId = ticket.AssignedTechnicianId;
        ticket.AssignedTechnicianId = technician.UserId;
        ticket.AssignedTechnician = technician;
        ticket.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.RepairOrders.Update(ticket);
        await _unitOfWork.TechnicianAssignmentHistories.AddAsync(new TechnicianAssignmentHistory
        {
            RepairOrderId = ticket.RepairOrderId,
            RepairOrder = ticket,
            AssignedByUserId = request.AssignedByUserId,
            PreviousTechnicianId = previousTechnicianId,
            AssignedTechnicianId = technician.UserId,
            AssignedAtUtc = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        var response = MapTicket(ticket, request.ActorRole, null);
        response.Message = "Technician assigned successfully.";
        return response;
    }

    public async Task<TechnicalTicketResponse> UpdateAsync(UpdateTechnicalTicketRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _updateValidator.ValidateAsync(request));
        EnsureUpdateRole(request.ActorRole);

        var ticket = await _unitOfWork.RepairOrders.GetWithDetailsAsync(request.RepairOrderId);
        if (ticket is null || IsClosed(ticket.Status))
        {
            throw new InvalidOperationException("Technical ticket cannot be updated.");
        }

        if (request.ActorRole.Equals(RoleConstants.Technician, StringComparison.OrdinalIgnoreCase) &&
            ticket.AssignedTechnicianId != request.ActorUserId)
        {
            throw new UnauthorizedException("Insufficient access rights.");
        }

        await ApplyRoleSpecificUpdatesAsync(ticket, request);
        ticket.TotalAmount = ticket.ServiceFee + ticket.RepairOrderDetails.Sum(detail => detail.LineTotal);
        ticket.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.RepairOrders.Update(ticket);

        await _unitOfWork.RepairOrderStatusHistories.AddAsync(new RepairOrderStatusHistory
        {
            RepairOrderId = ticket.RepairOrderId,
            RepairOrder = ticket,
            UpdatedByUserId = request.ActorUserId,
            Status = ticket.Status,
            Notes = BuildUpdateSummary(request),
            UpdatedAtUtc = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
        await TrySendUpdateNotificationAsync(ticket);

        var response = MapTicket(ticket, request.ActorRole, ticket.CustomerId);
        response.Message = "Technical ticket updated successfully.";
        return response;
    }

    public async Task<TechnicalTicketResponse> ConfirmPaymentAsync(ConfirmRepairPaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _paymentValidator.ValidateAsync(request));
        EnsureReceptionRole(request.ActorRole);

        var ticket = await _unitOfWork.RepairOrders.GetWithDetailsAsync(request.RepairOrderId);
        if (ticket is null)
        {
            throw new NotFoundException("Technical ticket not found.");
        }

        if (ticket.Payments.Any(payment => payment.PaymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Technical ticket has already been paid.");
        }

        if (ticket.Status.Equals(PendingDeliveryConfirmationStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Customer has not confirmed receipt of delivery, please wait for customer confirmation before payment.");
        }

        if (ticket.Status.Equals(PendingManualDeliveryStatus, StringComparison.OrdinalIgnoreCase))
        {
            ticket.Status = DeliveredStatus;
            ticket.DeliveryConfirmedAtUtc = DateTime.UtcNow;
            ticket.DeliveryConfirmationMethod = "Manual";
        }
        else if (!ticket.Status.Equals(DeliveredStatus, StringComparison.OrdinalIgnoreCase) &&
            !ticket.Status.Equals(PendingDeliveryStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only delivered technical tickets can be paid.");
        }

        ticket.Status = CompletedStatus;
        ticket.CompletedDate = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Payments.AddAsync(new Payment
        {
            CustomerId = ticket.CustomerId,
            RepairOrder = ticket,
            RepairOrderId = ticket.RepairOrderId,
            PaymentCode = $"RPAY-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            PaymentDate = DateTime.UtcNow,
            Amount = ticket.TotalAmount,
            PaymentMethod = request.PaymentMethod.Trim(),
            PaymentStatus = "Completed",
            Notes = request.ManualDeliveryNote,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _unitOfWork.RepairOrderStatusHistories.AddAsync(new RepairOrderStatusHistory
        {
            RepairOrderId = ticket.RepairOrderId,
            RepairOrder = ticket,
            UpdatedByUserId = request.ConfirmedByUserId,
            Status = CompletedStatus,
            Notes = $"Payment confirmed by {request.PaymentMethod}.",
            UpdatedAtUtc = DateTime.UtcNow
        });
        _unitOfWork.RepairOrders.Update(ticket);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(ticket.Customer?.Email))
        {
            await _emailSender.SendTechnicalTicketCompletedAsync(ticket.Customer.Email, ticket.Customer.FullName, ticket.RepairOrderCode);
        }

        var response = MapTicket(ticket, request.ActorRole, ticket.CustomerId);
        response.Message = "Payment confirmed successfully.";
        return response;
    }

    public async Task<TechnicalTicketResponse> CancelAsync(CancelTechnicalTicketRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _cancelValidator.ValidateAsync(request));
        EnsureCancelRole(request.ActorRole);

        var ticket = await _unitOfWork.RepairOrders.GetWithDetailsAsync(request.RepairOrderId);
        if (ticket is null)
        {
            throw new NotFoundException("Technical ticket not found.");
        }

        if (!CanCancelTicketStatus(ticket.Status))
        {
            throw new InvalidOperationException("Technical ticket cannot be cancelled in its current status.");
        }

        if (request.ActorRole.Equals(RoleConstants.Receptionist, StringComparison.OrdinalIgnoreCase) &&
            ticket.AssignedTechnicianId.HasValue)
        {
            throw new UnauthorizedException("Receptionist can only cancel unassigned technical tickets.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            ticket.Status = CancelledStatus;
            ticket.Notes = string.IsNullOrWhiteSpace(ticket.Notes)
                ? $"Cancellation reason: {request.Reason.Trim()}"
                : $"{ticket.Notes}; Cancellation reason: {request.Reason.Trim()}";
            ticket.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.RepairOrderStatusHistories.AddAsync(new RepairOrderStatusHistory
            {
                RepairOrderId = ticket.RepairOrderId,
                RepairOrder = ticket,
                UpdatedByUserId = request.ActorUserId,
                Status = CancelledStatus,
                Notes = request.Reason.Trim(),
                UpdatedAtUtc = DateTime.UtcNow
            });

            await _unitOfWork.SystemActivityLogs.AddAsync(new SystemActivityLog
            {
                ActorUserId = request.ActorUserId,
                FunctionName = "Cancel Technical Ticket",
                OperationType = "Cancel",
                AffectedData = ticket.RepairOrderCode,
                ExecutionResult = "Success",
                Details = request.Reason.Trim(),
                ExecutedAtUtc = DateTime.UtcNow
            });

            _unitOfWork.RepairOrders.Update(ticket);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        if (!string.IsNullOrWhiteSpace(ticket.Customer?.Email))
        {
            await _emailSender.SendTechnicalTicketCancelledAsync(ticket.Customer.Email, ticket.Customer.FullName, ticket.RepairOrderCode, request.Reason.Trim());
        }

        if (!string.IsNullOrWhiteSpace(ticket.AssignedTechnician?.Email))
        {
            await _emailSender.SendTechnicianTicketCancelledAsync(ticket.AssignedTechnician.Email, ticket.AssignedTechnician.FullName, ticket.RepairOrderCode, request.Reason.Trim());
        }

        var response = MapTicket(ticket, request.ActorRole, ticket.CustomerId);
        response.Message = "Technical ticket cancelled successfully.";
        return response;
    }

    private async Task<RepairOrder> GetAuthorizedTicketAsync(int repairOrderId, ViewTechnicalTicketsRequest viewer)
    {
        ValidateTicketId(repairOrderId);
        ThrowIfInvalid(await _viewValidator.ValidateAsync(viewer));
        var ticket = await _unitOfWork.RepairOrders.GetWithDetailsAsync(repairOrderId);
        if (ticket is null || !CanView(ticket, viewer))
        {
            throw new NotFoundException("Technical ticket not found.");
        }

        return ticket;
    }

    private async Task ApplyRoleSpecificUpdatesAsync(RepairOrder ticket, UpdateTechnicalTicketRequest request)
    {
        if (request.ActorRole.Equals(RoleConstants.Receptionist, StringComparison.OrdinalIgnoreCase))
        {
            ApplyCustomerUpdates(ticket, request);
            return;
        }

        if (request.ActorRole.Equals(RoleConstants.Technician, StringComparison.OrdinalIgnoreCase))
        {
            await ApplyTechnicalUpdatesAsync(ticket, request);
            return;
        }

        ApplyCustomerUpdates(ticket, request);
        await ApplyTechnicalUpdatesAsync(ticket, request);
    }

    private static void ApplyCustomerUpdates(RepairOrder ticket, UpdateTechnicalTicketRequest request)
    {
        if (ticket.Customer is null)
        {
            return;
        }

        ticket.Customer.FullName = Normalize(request.CustomerFullName) ?? ticket.Customer.FullName;
        ticket.Customer.Phone = Normalize(request.CustomerPhone) ?? ticket.Customer.Phone;
        ticket.Customer.Address = Normalize(request.CustomerAddress) ?? ticket.Customer.Address;
    }

    private async Task ApplyTechnicalUpdatesAsync(RepairOrder ticket, UpdateTechnicalTicketRequest request)
    {
        ticket.InspectionResult = Normalize(request.InspectionResult) ?? ticket.InspectionResult;
        ticket.Diagnosis = Normalize(request.Diagnosis) ?? ticket.Diagnosis;
        ticket.DeviceCondition = Normalize(request.DeviceCondition) ?? ticket.DeviceCondition;
        ticket.WorkPerformed = Normalize(request.WorkPerformed) ?? ticket.WorkPerformed;
        ticket.RepairResult = Normalize(request.RepairResult) ?? ticket.RepairResult;
        ticket.AccompanyingAccessories = Normalize(request.AccompanyingAccessories) ?? ticket.AccompanyingAccessories;
        ticket.ProcessingMinutes = request.ProcessingMinutes ?? ticket.ProcessingMinutes;
        ticket.ServiceFee = request.ServiceFee ?? ticket.ServiceFee;

        foreach (var component in request.UsedComponents)
        {
            var product = await _unitOfWork.Products.GetActiveProductByIdAsync(component.ProductId);
            if (product is null)
            {
                throw new NotFoundException("Component not found.");
            }

            if (component.Quantity > product.QuantityInStock)
            {
                throw new InvalidOperationException("Component stock is insufficient.");
            }

            product.QuantityInStock -= component.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Products.Update(product);

            ticket.RepairOrderDetails.Add(new RepairOrderDetail
            {
                RepairOrder = ticket,
                Product = product,
                ProductId = product.ProductId,
                Description = component.Notes ?? product.ProductName,
                Quantity = component.Quantity,
                UnitCost = product.SalePrice,
                LineTotal = product.SalePrice * component.Quantity
            });
        }

        var decision = Normalize(request.StatusDecision);
        if (decision is null && !string.IsNullOrWhiteSpace(request.InspectionResult))
        {
            ticket.Status = "Under Inspection";
        }
        else if (decision?.Equals("Pending Delivery", StringComparison.OrdinalIgnoreCase) == true)
        {
            ticket.Status = PendingDeliveryStatus;
        }
        else if (decision?.Equals("Complete Repair", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (string.IsNullOrWhiteSpace(ticket.RepairResult) || ticket.RepairOrderDetails.Count == 0)
            {
                throw new ValidationException(new[] { new ValidationFailure(nameof(request.StatusDecision), "Repair results and components are required before completing repair.") });
            }

            if (!string.IsNullOrWhiteSpace(ticket.Customer?.Email) || !string.IsNullOrWhiteSpace(ticket.Customer?.Phone))
            {
                ticket.Status = PendingDeliveryConfirmationStatus;
                ticket.PendingDeliveryConfirmationAtUtc = DateTime.UtcNow;
                ticket.DeliveryConfirmationDeadlineUtc = DateTime.UtcNow.AddDays(7);
                var otp = _otpGenerator.Generate(6);
                ticket.DeliveryOtpHash = _passwordHasher.Hash(otp);
                ticket.DeliveryOtpExpiresAtUtc = DateTime.UtcNow.AddMinutes(15);
                ticket.DeliveryOtpAttemptCount = 0;
                ticket.DeliveryOtpSentAtUtc = DateTime.UtcNow;
                ticket.DeliveryOtpSentToEmail = ticket.Customer?.Email;
                ticket.DeliveryOtpSentToPhone = ticket.Customer?.Phone;
                if (!string.IsNullOrWhiteSpace(ticket.Customer?.Email))
                {
                    try
                    {
                        await _emailSender.SendDeliveryConfirmationOtpAsync(ticket.Customer.Email, ticket.Customer.FullName, ticket.RepairOrderCode, otp);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogWarning(exception, "Failed to send delivery OTP for ticket {TicketCode}.", ticket.RepairOrderCode);
                    }
                }
            }
            else
            {
                ticket.Status = PendingManualDeliveryStatus;
            }
        }
    }

    private async Task TrySendUpdateNotificationAsync(RepairOrder ticket)
    {
        if (ticket.Status.Equals(CompletedStatus, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(ticket.Customer?.Email))
        {
            return;
        }

        await _emailSender.SendTechnicalTicketCreatedAsync(ticket.Customer.Email, ticket.Customer.FullName, ticket.RepairOrderCode);
    }

    private static string BuildUpdateSummary(UpdateTechnicalTicketRequest request)
    {
        return $"{request.ActorRole} updated ticket.";
    }

    private static TechnicalTicketResponse MapTicket(RepairOrder ticket, string actorRole, int? viewerCustomerId)
    {
        var isOwner = actorRole.Equals(RoleConstants.Customer, StringComparison.OrdinalIgnoreCase) &&
            viewerCustomerId == ticket.CustomerId;
        var isStaffOtpViewer = actorRole.Equals(RoleConstants.Receptionist, StringComparison.OrdinalIgnoreCase) ||
            actorRole.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase);

        return new TechnicalTicketResponse
        {
            RepairOrderId = ticket.RepairOrderId,
            TicketCode = ticket.RepairOrderCode,
            CustomerId = ticket.CustomerId,
            CustomerName = ticket.Customer?.FullName ?? string.Empty,
            CustomerEmail = ticket.Customer?.Email,
            CustomerPhone = ticket.Customer?.Phone,
            DeviceType = ticket.DeviceType,
            Brand = ticket.Brand,
            DeviceModel = ticket.DeviceModel,
            RequestType = ticket.RequestType,
            IssueDescription = ticket.IssueDescription,
            DeviceCondition = ticket.DeviceCondition,
            Status = ticket.Status,
            AssignedTechnicianId = ticket.AssignedTechnicianId,
            AssignedTechnicianName = ticket.AssignedTechnician?.FullName,
            IsHandedOverToTechnician = HasBeenHandedOverToTechnician(ticket),
            DeliveryConfirmationDeadlineUtc = ticket.DeliveryConfirmationDeadlineUtc,
            ShowConfirmDeliveryButton = isOwner && ticket.Status == PendingDeliveryConfirmationStatus,
            DeliveryOtpSentAtUtc = isStaffOtpViewer ? ticket.DeliveryOtpSentAtUtc : null,
            DeliveryOtpSentToEmail = isStaffOtpViewer ? ticket.DeliveryOtpSentToEmail : null,
            DeliveryOtpSentToPhone = isStaffOtpViewer ? ticket.DeliveryOtpSentToPhone : null
        };
    }

    private static bool CanView(RepairOrder ticket, ViewTechnicalTicketsRequest viewer)
    {
        if (viewer.ActorRole.Equals(RoleConstants.Customer, StringComparison.OrdinalIgnoreCase))
        {
            return viewer.CustomerId == ticket.CustomerId;
        }

        if (viewer.ActorRole.Equals(RoleConstants.Technician, StringComparison.OrdinalIgnoreCase))
        {
            return ticket.AssignedTechnicianId == viewer.ActorUserId;
        }

        return viewer.ActorRole.Equals(RoleConstants.Receptionist, StringComparison.OrdinalIgnoreCase) ||
            viewer.ActorRole.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase) ||
            viewer.ActorRole.Equals(RoleConstants.Admin, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasBeenHandedOverToTechnician(RepairOrder ticket)
    {
        if (!ticket.AssignedTechnicianId.HasValue)
        {
            return false;
        }

        if (ticket.Status.Equals(HandedOverToTechnicianStatus, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (ticket.StatusHistories.Any(history => history.Status.Equals(HandedOverToTechnicianStatus, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return !ticket.Status.Equals(PendingReceptionStatus, StringComparison.OrdinalIgnoreCase) &&
            !ticket.Status.Equals(ReceivedStatus, StringComparison.OrdinalIgnoreCase);
    }

    private static void EnsureReceptionRole(string role)
    {
        if (!role.Equals(RoleConstants.Receptionist, StringComparison.OrdinalIgnoreCase) &&
            !role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Receptionist or Manager roles can perform this function.");
        }
    }

    private static void EnsureCancelRole(string role)
    {
        if (!role.Equals(RoleConstants.Receptionist, StringComparison.OrdinalIgnoreCase) &&
            !role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase) &&
            !role.Equals(RoleConstants.Admin, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Receptionist, Manager, or Administrator roles can cancel technical tickets.");
        }
    }

    private static bool CanCancelTicketStatus(string status)
    {
        return status.Equals(PendingReceptionStatus, StringComparison.OrdinalIgnoreCase) ||
            status.Equals(HandedOverToTechnicianStatus, StringComparison.OrdinalIgnoreCase) ||
            status.Equals("Pending Receipt", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("Under Inspection", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("Pending Assignment", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("Under Repair", StringComparison.OrdinalIgnoreCase);
    }

    private static void EnsureUpdateRole(string role)
    {
        if (!role.Equals(RoleConstants.Receptionist, StringComparison.OrdinalIgnoreCase) &&
            !role.Equals(RoleConstants.Technician, StringComparison.OrdinalIgnoreCase) &&
            !role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Receptionist, Technician, or Manager roles can perform this function.");
        }
    }

    private static bool IsClosed(string status)
    {
        return status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("Delivered", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("Canceled", StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateTicketCode()
    {
        return $"TT-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateTicketId(int repairOrderId)
    {
        if (repairOrderId <= 0)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(repairOrderId), "Repair order id must be greater than 0.") });
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
