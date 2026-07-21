using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Order;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
[Route("api/sales-orders")]
public class OrdersController : AuthenticatedControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IUnitOfWork _unitOfWork;

    public OrdersController(IOrderService orderService, IUnitOfWork unitOfWork)
    {
        _orderService = orderService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetSalesOrders([FromQuery] int? customerId)
    {
        var orders = customerId.HasValue
            ? await _unitOfWork.SalesOrders.GetByCustomerIdAsync(customerId.Value)
            : await _unitOfWork.SalesOrders.GetAllWithDetailsAsync();
        var data = orders.Select(order => MapOrder(order, order.Payments.FirstOrDefault()?.PaymentMethod ?? string.Empty, string.Empty)).ToArray();
        return Ok(new ApiResponse<IReadOnlyCollection<OrderResponse>>
        {
            Success = true,
            Message = data.Length == 0 ? "No sales orders found." : "Sales orders retrieved successfully.",
            Data = data
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSalesOrder(int id)
    {
        var order = await _unitOfWork.SalesOrders.GetWithDetailsAsync(id);
        if (order is null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Sales order not found." });
        }

        var response = MapOrder(order, order.Payments.FirstOrDefault()?.PaymentMethod ?? string.Empty, "Sales order retrieved successfully.");
        return Ok(new ApiResponse<OrderResponse> { Success = true, Message = response.Message, Data = response });
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var result = await _orderService.PlaceOrderAsync(request);
        return Ok(new ApiResponse<OrderResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Receptionist,Manager")]
    [HttpPost("sales")]
    public async Task<IActionResult> ProcessSalesOrder([FromBody] ProcessSalesOrderRequest request)
    {
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _orderService.ProcessSalesOrderAsync(request);
        return Ok(new ApiResponse<OrderResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderRequest request)
    {
        request.SalesOrderId = id;
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _orderService.CancelOrderAsync(request);
        return Ok(new ApiResponse<OrderResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Receptionist,Manager")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateSalesOrderStatus(int id, [FromBody] UpdateSalesOrderStatusRequest request)
    {
        var order = await _unitOfWork.SalesOrders.GetWithDetailsAsync(id);
        if (order is null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Sales order not found." });
        }

        order.Status = request.Status.Trim();
        order.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.SalesOrders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        var response = MapOrder(order, order.Payments.FirstOrDefault()?.PaymentMethod ?? string.Empty, "Sales order status updated successfully.");
        return Ok(new ApiResponse<OrderResponse> { Success = true, Message = response.Message, Data = response });
    }

    [Authorize(Roles = "Receptionist,Manager")]
    [HttpPost("{id:int}/payment")]
    public async Task<IActionResult> AddSalesOrderPayment(int id, [FromBody] SalesOrderPaymentRequest request)
    {
        var order = await _unitOfWork.SalesOrders.GetWithDetailsAsync(id);
        if (order is null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Sales order not found." });
        }

        var amount = request.Amount <= 0 ? order.TotalAmount : request.Amount;
        await _unitOfWork.Payments.AddAsync(new Payment
        {
            CustomerId = order.CustomerId,
            SalesOrderId = order.SalesOrderId,
            SalesOrder = order,
            PaymentCode = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            PaymentDate = DateTime.UtcNow,
            Amount = amount,
            PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod.Trim(),
            PaymentStatus = string.IsNullOrWhiteSpace(request.PaymentStatus) ? "Completed" : request.PaymentStatus.Trim(),
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        if (request.MarkOrderCompleted)
        {
            order.Status = "Completed";
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.SalesOrders.Update(order);
        }

        await _unitOfWork.SaveChangesAsync();
        var response = MapOrder(order, request.PaymentMethod, "Sales order payment recorded successfully.");
        return Ok(new ApiResponse<OrderResponse> { Success = true, Message = response.Message, Data = response });
    }

    private static OrderResponse MapOrder(SalesOrder order, string paymentMethod, string message)
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

    public sealed class UpdateSalesOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public sealed class SalesOrderPaymentRequest
    {
        public string PaymentMethod { get; set; } = "Cash";
        public string PaymentStatus { get; set; } = "Completed";
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public bool MarkOrderCompleted { get; set; } = true;
    }
}
