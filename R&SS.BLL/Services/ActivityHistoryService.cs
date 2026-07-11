using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.DTOs.Activity;
using R_SS.BLL.DTOs.Order;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class ActivityHistoryService : IActivityHistoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<ActivityHistoryRequest> _validator;
    private readonly ILogger<ActivityHistoryService> _logger;

    public ActivityHistoryService(
        IUnitOfWork unitOfWork,
        IValidator<ActivityHistoryRequest> validator,
        ILogger<ActivityHistoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Gets order and technical service activity history for a customer.
    /// </summary>
    public async Task<ActivityHistoryResponse> GetHistoryAsync(ActivityHistoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _validator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
        if (customer is null)
        {
            throw new NotFoundException("Customer not found.");
        }

        var type = request.Type.Trim().ToLower();
        var activities = new List<ActivityItemResponse>();

        if (type is "all" or "order")
        {
            var orders = await _unitOfWork.SalesOrders.GetByCustomerIdAsync(request.CustomerId);
            activities.AddRange(orders.Select(order => new ActivityItemResponse
            {
                Type = "order",
                Id = order.SalesOrderId,
                Code = order.SalesOrderCode,
                CreatedAt = order.CreatedAt,
                Status = order.Status
            }));
        }

        if (type is "all" or "technical")
        {
            var repairOrders = await _unitOfWork.RepairOrders.GetSubmittedByCustomerIdAsync(request.CustomerId, request.IncludeCanceled);
            activities.AddRange(repairOrders.Select(order => new ActivityItemResponse
            {
                Type = "technical",
                Id = order.RepairOrderId,
                Code = order.RepairOrderCode,
                CreatedAt = order.CreatedAt,
                Status = order.Status
            }));
        }

        var orderedActivities = activities
            .OrderByDescending(activity => activity.CreatedAt)
            .ToArray();

        _logger.LogInformation("Retrieved {ActivityCount} activities for customer {CustomerId}.", orderedActivities.Length, request.CustomerId);

        return new ActivityHistoryResponse
        {
            Activities = orderedActivities,
            Message = orderedActivities.Length == 0 ? "There are no activities." : "Activity history retrieved successfully."
        };
    }

    /// <summary>
    /// Gets a sales order detail owned by the customer.
    /// </summary>
    public async Task<OrderResponse> GetSalesOrderDetailsAsync(int customerId, int salesOrderId)
    {
        ValidateIds(customerId, salesOrderId);

        var order = await _unitOfWork.SalesOrders.GetWithDetailsAsync(salesOrderId);
        if (order is null || order.CustomerId != customerId)
        {
            throw new NotFoundException("Sales order not found.");
        }

        return new OrderResponse
        {
            SalesOrderId = order.SalesOrderId,
            SalesOrderCode = order.SalesOrderCode,
            CustomerId = order.CustomerId,
            Status = order.Status,
            SubTotal = order.SubTotal,
            DiscountAmount = order.DiscountAmount,
            TaxAmount = order.TaxAmount,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.Payments.FirstOrDefault()?.PaymentMethod ?? string.Empty,
            Items = order.SalesOrderDetails.Select(detail => new OrderItemResponse
            {
                ProductId = detail.ProductId,
                ProductName = detail.Product?.ProductName ?? string.Empty,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                DiscountAmount = detail.DiscountAmount,
                LineTotal = detail.LineTotal
            }).ToArray(),
            Message = "Sales order details retrieved successfully."
        };
    }

    /// <summary>
    /// Gets a repair order detail owned by the customer.
    /// </summary>
    public async Task<RepairOrderDetailResponse> GetRepairOrderDetailsAsync(int customerId, int repairOrderId)
    {
        ValidateIds(customerId, repairOrderId);

        var order = await _unitOfWork.RepairOrders.GetWithDetailsAsync(repairOrderId);
        if (order is null || order.CustomerId != customerId)
        {
            throw new NotFoundException("Repair order not found.");
        }

        return new RepairOrderDetailResponse
        {
            RepairOrderId = order.RepairOrderId,
            RepairOrderCode = order.RepairOrderCode,
            Status = order.Status,
            IssueDescription = order.IssueDescription,
            TotalAmount = order.TotalAmount,
            Message = "Repair order details retrieved successfully."
        };
    }

    private static void ValidateIds(int customerId, int activityId)
    {
        var failures = new List<ValidationFailure>();
        if (customerId <= 0)
        {
            failures.Add(new ValidationFailure(nameof(customerId), "Customer id must be greater than 0."));
        }

        if (activityId <= 0)
        {
            failures.Add(new ValidationFailure(nameof(activityId), "Activity id must be greater than 0."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
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
