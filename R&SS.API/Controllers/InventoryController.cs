using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Inventory;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize(Roles = "Manager,Receptionist")]
[Route("api/inventory")]
public class InventoryController : AuthenticatedControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IInventoryStatisticService _inventoryStatisticService;

    public InventoryController(IInventoryService inventoryService, IInventoryStatisticService inventoryStatisticService)
    {
        _inventoryService = inventoryService;
        _inventoryStatisticService = inventoryStatisticService;
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> ApplyTransaction([FromBody] InventoryTransactionRequest request)
    {
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _inventoryService.ApplyTransactionAsync(request);
        return Ok(new ApiResponse<InventoryTransactionResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPost("import")]
    public Task<IActionResult> ImportInventory([FromBody] InventoryTransactionRequest request)
    {
        request.TransactionType = "Receipt";
        return ApplyTransaction(request);
    }

    [HttpPost("export")]
    public Task<IActionResult> ExportInventory([FromBody] InventoryTransactionRequest request)
    {
        request.TransactionType = "Issue";
        return ApplyTransaction(request);
    }

    [HttpPost("adjust")]
    public Task<IActionResult> AdjustInventory([FromBody] InventoryTransactionRequest request)
    {
        request.TransactionType = "Adjustment";
        return ApplyTransaction(request);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] InventoryHistoryRequest request)
    {
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _inventoryService.GetHistoryAsync(request);
        return Ok(new ApiResponse<InventoryHistoryResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] InventoryStatisticRequest request)
    {
        request.ActorRole = CurrentRole();
        var result = await _inventoryStatisticService.GenerateAsync(request);
        return Ok(new ApiResponse<InventoryStatisticResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpGet("low-stock-warning")]
    public async Task<IActionResult> GetLowStockWarning([FromQuery] InventoryStatisticRequest request)
    {
        request.ActorRole = CurrentRole();
        request.StockStatus = "low";
        var result = await _inventoryStatisticService.GenerateAsync(request);
        return Ok(new ApiResponse<InventoryStatisticResponse> { Success = true, Message = result.Message, Data = result });
    }
}
