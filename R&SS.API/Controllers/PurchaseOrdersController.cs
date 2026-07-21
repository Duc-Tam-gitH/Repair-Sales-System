using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize(Roles = "Manager")]
[Route("api/purchase-orders")]
public class PurchaseOrdersController : AuthenticatedControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseOrdersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetPurchaseOrders()
    {
        var orders = await _unitOfWork.PurchaseOrders.GetAllAsync();
        var data = orders.OrderByDescending(order => order.CreatedAt).ToArray();
        return Ok(new ApiResponse<IReadOnlyCollection<PurchaseOrder>>
        {
            Success = true,
            Message = data.Length == 0 ? "No purchase orders found." : "Purchase orders retrieved successfully.",
            Data = data
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePurchaseOrder([FromBody] PurchaseOrder request)
    {
        request.PurchaseOrderId = 0;
        request.PurchaseOrderCode = string.IsNullOrWhiteSpace(request.PurchaseOrderCode)
            ? $"PO-{DateTime.UtcNow:yyyyMMddHHmmssfff}"
            : request.PurchaseOrderCode.Trim();
        request.CreatedByUserId = CurrentUserId();
        request.OrderDate = request.OrderDate == default ? DateTime.UtcNow : request.OrderDate;
        request.CreatedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        foreach (var detail in request.PurchaseOrderDetails)
        {
            detail.PurchaseOrderId = 0;
            detail.LineTotal = detail.Quantity * detail.UnitCost;
        }

        request.TotalAmount = request.PurchaseOrderDetails.Sum(detail => detail.LineTotal);
        await _unitOfWork.PurchaseOrders.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new ApiResponse<PurchaseOrder>
        {
            Success = true,
            Message = "Purchase order created successfully.",
            Data = request
        });
    }
}
