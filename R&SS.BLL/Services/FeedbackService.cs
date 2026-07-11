using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Feedback;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<SubmitFeedbackRequest> _validator;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(
        IUnitOfWork unitOfWork,
        IValidator<SubmitFeedbackRequest> validator,
        ILogger<FeedbackService> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<EligibleFeedbackListResponse> GetEligibleItemsAsync(int customerId)
    {
        if (customerId <= 0)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(customerId), "Customer id must be greater than 0.") });
        }

        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer is null)
        {
            throw new NotFoundException("Customer not found.");
        }

        var items = new List<EligibleFeedbackItemResponse>();
        var orders = await _unitOfWork.SalesOrders.GetByCustomerIdAsync(customerId);
        foreach (var order in orders.Where(order => order.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
        {
            if (!await _unitOfWork.ServiceFeedbacks.ExistsForSalesOrderAsync(order.SalesOrderId))
            {
                items.Add(new EligibleFeedbackItemResponse
                {
                    Type = "order",
                    Id = order.SalesOrderId,
                    Code = order.SalesOrderCode,
                    CompletedAtUtc = order.UpdatedAt
                });
            }
        }

        var tickets = await _unitOfWork.RepairOrders.GetSubmittedByCustomerIdAsync(customerId, includeCanceled: false);
        foreach (var ticket in tickets.Where(ticket => ticket.Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase)))
        {
            if (!await _unitOfWork.ServiceFeedbacks.ExistsForRepairOrderAsync(ticket.RepairOrderId))
            {
                items.Add(new EligibleFeedbackItemResponse
                {
                    Type = "technical",
                    Id = ticket.RepairOrderId,
                    Code = ticket.RepairOrderCode,
                    CompletedAtUtc = ticket.DeliveryConfirmedAtUtc ?? ticket.UpdatedAt
                });
            }
        }

        return new EligibleFeedbackListResponse
        {
            Items = items.OrderByDescending(item => item.CompletedAtUtc).ToArray(),
            Message = items.Count == 0 ? "No eligible items for rating." : "Eligible feedback items retrieved successfully."
        };
    }

    public async Task<FeedbackResponse> SubmitAsync(SubmitFeedbackRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _validator.ValidateAsync(request));

        var feedback = request.Type.Equals("order", StringComparison.OrdinalIgnoreCase)
            ? await BuildOrderFeedbackAsync(request)
            : await BuildTechnicalFeedbackAsync(request);

        await _unitOfWork.ServiceFeedbacks.AddAsync(feedback);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Customer {CustomerId} submitted feedback.", request.CustomerId);

        return new FeedbackResponse
        {
            FeedbackId = feedback.ServiceFeedbackId,
            Rating = feedback.Rating,
            Comment = feedback.Comment,
            Message = "Feedback sent successfully."
        };
    }

    public async Task<FeedbackStatisticsResponse> GetStatisticsAsync(string actorRole)
    {
        if (!actorRole.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Managers can view feedback statistics.");
        }

        var feedbacks = await _unitOfWork.ServiceFeedbacks.GetAllWithReferencesAsync();
        return new FeedbackStatisticsResponse
        {
            TotalFeedbackCount = feedbacks.Count,
            AverageRating = feedbacks.Count == 0 ? 0 : feedbacks.Average(feedback => feedback.Rating),
            Message = "Feedback statistics retrieved successfully."
        };
    }

    private async Task<ServiceFeedback> BuildOrderFeedbackAsync(SubmitFeedbackRequest request)
    {
        var order = await _unitOfWork.SalesOrders.GetWithDetailsAsync(request.ItemId);
        if (order is null || order.CustomerId != request.CustomerId || !order.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Order is not eligible for feedback.");
        }

        if (await _unitOfWork.ServiceFeedbacks.ExistsForSalesOrderAsync(order.SalesOrderId))
        {
            throw new InvalidOperationException("This item has already been rated.");
        }

        return new ServiceFeedback
        {
            CustomerId = request.CustomerId,
            SalesOrderId = order.SalesOrderId,
            SalesOrder = order,
            Rating = request.Rating,
            Comment = Normalize(request.Comment),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private async Task<ServiceFeedback> BuildTechnicalFeedbackAsync(SubmitFeedbackRequest request)
    {
        var ticket = await _unitOfWork.RepairOrders.GetWithDetailsAsync(request.ItemId);
        if (ticket is null || ticket.CustomerId != request.CustomerId || !ticket.Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Technical ticket is not eligible for feedback.");
        }

        if (await _unitOfWork.ServiceFeedbacks.ExistsForRepairOrderAsync(ticket.RepairOrderId))
        {
            throw new InvalidOperationException("This item has already been rated.");
        }

        return new ServiceFeedback
        {
            CustomerId = request.CustomerId,
            RepairOrderId = ticket.RepairOrderId,
            RepairOrder = ticket,
            Rating = request.Rating,
            Comment = Normalize(request.Comment),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ThrowIfInvalid(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
