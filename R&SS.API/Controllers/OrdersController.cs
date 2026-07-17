using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Order;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersController : AuthenticatedControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
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
}
