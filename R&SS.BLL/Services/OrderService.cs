using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Order;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class OrderService : IOrderService
{
    private const string PendingConfirmationStatus = "Pending Confirmation";
    private const string BeingPreparedStatus = "Being Prepared";
    private const string CompletedStatus = "Completed";
    private const string CancelledStatus = "Cancelled";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<PlaceOrderRequest> _placeOrderValidator;
    private readonly IValidator<ProcessSalesOrderRequest> _processSalesOrderValidator;
    private readonly IValidator<CancelOrderRequest> _cancelOrderValidator;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        IValidator<PlaceOrderRequest> placeOrderValidator,
        IValidator<ProcessSalesOrderRequest> processSalesOrderValidator,
        IValidator<CancelOrderRequest> cancelOrderValidator,
        ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _placeOrderValidator = placeOrderValidator;
        _processSalesOrderValidator = processSalesOrderValidator;
        _cancelOrderValidator = cancelOrderValidator;
        _logger = logger;
    }

    /// <summary>
    /// Places an order from the customer's cart.
    /// </summary>
    public async Task<OrderResponse> PlaceOrderAsync(PlaceOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _placeOrderValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
        if (customer is null)
        {
            throw new NotFoundException("Customer not found.");
        }

        var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(request.CustomerId);
        if (cart is null || cart.CartItems.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty.");
        }

        EnsureCartStock(cart.CartItems);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var order = BuildSalesOrder(
                request.CustomerId,
                null,
                PendingConfirmationStatus,
                request.PaymentMethod,
                cart.CartItems.Select(item => new OrderItemSnapshot(item.Product!, item.Quantity)).ToArray(),
                0,
                $"Recipient: {request.RecipientName.Trim()}; Delivery: {request.DeliveryAddress.Trim()}");

            await _unitOfWork.SalesOrders.AddAsync(order);
            await _unitOfWork.Payments.AddAsync(BuildPayment(request.CustomerId, order, request.PaymentMethod, "Pending"));
            DecreaseStock(order.SalesOrderDetails);
            _unitOfWork.CartItems.DeleteRange(cart.CartItems);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Customer {CustomerId} placed order {OrderCode}.", request.CustomerId, order.SalesOrderCode);

            return BuildOrderResponse(order, request.PaymentMethod, "Order placed successfully.");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Processes an in-store sales order for staff.
    /// </summary>
    public async Task<OrderResponse> ProcessSalesOrderAsync(ProcessSalesOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _processSalesOrderValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);
        EnsureSalesRole(request.ActorRole);

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
        if (customer is null)
        {
            throw new NotFoundException("Customer not found.");
        }

        var snapshots = new List<OrderItemSnapshot>();
        foreach (var item in request.Items)
        {
            var product = await _unitOfWork.Products.GetActiveProductByIdAsync(item.ProductId);
            if (product is null)
            {
                throw new NotFoundException("Product not found.");
            }

            if (item.Quantity > product.QuantityInStock)
            {
                throw new InvalidOperationException("Stock is insufficient.");
            }

            snapshots.Add(new OrderItemSnapshot(product, item.Quantity));
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var order = BuildSalesOrder(
                request.CustomerId,
                request.ActorUserId,
                CompletedStatus,
                request.PaymentMethod,
                snapshots,
                request.DiscountAmount,
                null);

            await _unitOfWork.SalesOrders.AddAsync(order);
            await _unitOfWork.Payments.AddAsync(BuildPayment(request.CustomerId, order, request.PaymentMethod, "Completed"));
            DecreaseStock(order.SalesOrderDetails);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("User {UserId} processed sales order {OrderCode}.", request.ActorUserId, order.SalesOrderCode);

            return BuildOrderResponse(order, request.PaymentMethod, "Sales order processed successfully.");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponse> CancelOrderAsync(CancelOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _cancelOrderValidator.ValidateAsync(request));
        EnsureCancelRole(request.ActorRole);

        var order = await _unitOfWork.SalesOrders.GetWithDetailsAsync(request.SalesOrderId);
        if (order is null)
        {
            throw new NotFoundException("Sales order not found.");
        }

        if (!CanCancel(order.Status) || order.Payments.Any(payment => payment.PaymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Order cannot be cancelled.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            order.Status = CancelledStatus;
            order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                ? $"Cancellation reason: {request.Reason.Trim()}"
                : $"{order.Notes}; Cancellation reason: {request.Reason.Trim()}";
            order.UpdatedAt = DateTime.UtcNow;

            foreach (var detail in order.SalesOrderDetails)
            {
                if (detail.Product is null)
                {
                    throw new InvalidOperationException("Inventory restoration failed.");
                }

                detail.Product.QuantityInStock += detail.Quantity;
                detail.Product.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Products.Update(detail.Product);
            }

            await _unitOfWork.SystemActivityLogs.AddAsync(new SystemActivityLog
            {
                ActorUserId = request.ActorUserId,
                FunctionName = "Cancel Order",
                OperationType = "Cancel",
                AffectedData = order.SalesOrderCode,
                ExecutionResult = "Success",
                Details = request.Reason.Trim(),
                ExecutedAtUtc = DateTime.UtcNow
            });

            _unitOfWork.SalesOrders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        if (!string.IsNullOrWhiteSpace(order.Customer?.Email))
        {
            await _emailSender.SendOrderCancelledAsync(order.Customer.Email, order.Customer.FullName, order.SalesOrderCode, request.Reason.Trim());
        }

        _logger.LogInformation("User {ActorUserId} cancelled sales order {OrderCode}.", request.ActorUserId, order.SalesOrderCode);
        return BuildOrderResponse(order, order.Payments.FirstOrDefault()?.PaymentMethod ?? string.Empty, "Order cancelled successfully.");
    }

    private static void EnsureCartStock(IEnumerable<CartItem> items)
    {
        foreach (var item in items)
        {
            if (item.Product is null)
            {
                throw new NotFoundException("Product not found.");
            }

            if (item.Quantity > item.Product.QuantityInStock)
            {
                throw new InvalidOperationException("Stock is insufficient.");
            }
        }
    }

    private static SalesOrder BuildSalesOrder(
        int customerId,
        int? actorUserId,
        string status,
        string paymentMethod,
        IReadOnlyCollection<OrderItemSnapshot> items,
        decimal discountAmount,
        string? notes)
    {
        var subTotal = items.Sum(item => item.Product.SalePrice * item.Quantity);
        var totalAmount = Math.Max(0, subTotal - discountAmount);
        var order = new SalesOrder
        {
            CustomerId = customerId,
            CreatedByUserId = actorUserId,
            SalesOrderCode = CreateOrderCode(),
            SalesDate = DateTime.UtcNow,
            Status = status,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TaxAmount = 0,
            TotalAmount = totalAmount,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var item in items)
        {
            order.SalesOrderDetails.Add(new SalesOrderDetail
            {
                SalesOrder = order,
                Product = item.Product,
                ProductId = item.Product.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product.SalePrice,
                DiscountAmount = 0,
                LineTotal = item.Product.SalePrice * item.Quantity
            });
        }

        return order;
    }

    private static Payment BuildPayment(int customerId, SalesOrder order, string paymentMethod, string status)
    {
        return new Payment
        {
            CustomerId = customerId,
            SalesOrder = order,
            PaymentCode = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = paymentMethod.Trim(),
            PaymentStatus = status,
            Amount = order.TotalAmount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static void DecreaseStock(IEnumerable<SalesOrderDetail> details)
    {
        foreach (var detail in details)
        {
            if (detail.Product is not null)
            {
                detail.Product.QuantityInStock -= detail.Quantity;
                detail.Product.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    private static OrderResponse BuildOrderResponse(SalesOrder order, string paymentMethod, string message)
    {
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
            PaymentMethod = paymentMethod,
            Items = order.SalesOrderDetails.Select(detail => new OrderItemResponse
            {
                ProductId = detail.ProductId,
                ProductName = detail.Product?.ProductName ?? string.Empty,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                DiscountAmount = detail.DiscountAmount,
                LineTotal = detail.LineTotal
            }).ToArray(),
            Message = message
        };
    }

    private static void EnsureSalesRole(string role)
    {
        if (!string.Equals(role, RoleConstants.Receptionist, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role, RoleConstants.Manager, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role, RoleConstants.Admin, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Receptionist, Manager, or Administrator roles can process sales orders.");
        }
    }

    private static void EnsureCancelRole(string role)
    {
        if (!string.Equals(role, RoleConstants.Manager, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role, RoleConstants.Admin, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Manager or Administrator roles can cancel orders.");
        }
    }

    private static bool CanCancel(string status)
    {
        return status.Equals(PendingConfirmationStatus, StringComparison.OrdinalIgnoreCase) ||
            status.Equals(BeingPreparedStatus, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateOrderCode()
    {
        return $"SO-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    private static void ThrowIfInvalid(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }

    private sealed record OrderItemSnapshot(Product Product, int Quantity);
}
