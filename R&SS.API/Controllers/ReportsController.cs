using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Inventory;
using R_SS.BLL.DTOs.Report;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize(Roles = "Manager")]
[Route("api/reports")]
public class ReportsController : AuthenticatedControllerBase
{
    private readonly IRevenueReportService _revenueReportService;
    private readonly IInventoryStatisticService _inventoryStatisticService;
    private readonly IUnitOfWork _unitOfWork;

    public ReportsController(
        IRevenueReportService revenueReportService,
        IInventoryStatisticService inventoryStatisticService,
        IUnitOfWork unitOfWork)
    {
        _revenueReportService = revenueReportService;
        _inventoryStatisticService = inventoryStatisticService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] RevenueReportRequest request)
    {
        request.ActorRole = CurrentRole();
        var result = await _revenueReportService.GenerateAsync(request);
        return Ok(new ApiResponse<RevenueReportResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpGet("repairs")]
    public async Task<IActionResult> GetRepairsReport()
    {
        var repairs = await _unitOfWork.RepairOrders.GetAllAsync();
        var data = repairs.ToArray();
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = data.Length == 0 ? "No repair records found." : "Repair report retrieved successfully.",
            Data = new
            {
                totalRepairs = data.Length,
                completedRepairs = data.Count(repair => repair.CompletedDate.HasValue || repair.Status.Contains("Completed", StringComparison.OrdinalIgnoreCase)),
                repairs = data
            }
        });
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport([FromQuery] InventoryStatisticRequest request)
    {
        request.ActorRole = CurrentRole();
        var result = await _inventoryStatisticService.GenerateAsync(request);
        return Ok(new ApiResponse<InventoryStatisticResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpGet("top-selling-products")]
    public async Task<IActionResult> GetTopSellingProducts()
    {
        var orders = await _unitOfWork.SalesOrders.GetAllWithDetailsAsync();
        var data = orders
            .SelectMany(order => order.SalesOrderDetails)
            .GroupBy(detail => new { detail.ProductId, ProductName = detail.Product?.ProductName ?? string.Empty })
            .Select(group => new
            {
                group.Key.ProductId,
                group.Key.ProductName,
                quantitySold = group.Sum(detail => detail.Quantity),
                revenue = group.Sum(detail => detail.LineTotal)
            })
            .OrderByDescending(item => item.quantitySold)
            .ToArray();
        return Ok(new ApiResponse<object> { Success = true, Message = "Top-selling products retrieved successfully.", Data = data });
    }

    [HttpGet("technician-performance")]
    public async Task<IActionResult> GetTechnicianPerformance()
    {
        var repairs = await _unitOfWork.RepairOrders.GetAllAsync();
        var data = repairs
            .Where(repair => repair.AssignedTechnicianId.HasValue)
            .GroupBy(repair => repair.AssignedTechnicianId!.Value)
            .Select(group => new
            {
                technicianId = group.Key,
                assigned = group.Count(),
                completed = group.Count(repair => repair.CompletedDate.HasValue || repair.Status.Contains("Completed", StringComparison.OrdinalIgnoreCase))
            })
            .OrderByDescending(item => item.completed)
            .ToArray();
        return Ok(new ApiResponse<object> { Success = true, Message = "Technician performance retrieved successfully.", Data = data });
    }

    [HttpGet("customers-summary")]
    public async Task<IActionResult> GetCustomersSummary()
    {
        var customers = (await _unitOfWork.Customers.GetAllAsync()).ToArray();
        var orders = await _unitOfWork.SalesOrders.GetAllWithDetailsAsync();
        var repairs = (await _unitOfWork.RepairOrders.GetAllAsync()).ToArray();
        var data = new
        {
            totalCustomers = customers.Length,
            activeCustomers = customers.Count(customer => customer.IsActive),
            customersWithOrders = orders.Select(order => order.CustomerId).Distinct().Count(),
            customersWithRepairs = repairs.Select(repair => repair.CustomerId).Distinct().Count()
        };
        return Ok(new ApiResponse<object> { Success = true, Message = "Customer summary retrieved successfully.", Data = data });
    }

    [HttpPost("export-pdf")]
    public IActionResult ExportPdf()
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new ApiResponse<object>
        {
            Success = false,
            Message = "PDF export is not implemented by the current reporting service."
        });
    }
}
