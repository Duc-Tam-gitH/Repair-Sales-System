using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Supplier;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize(Roles = "Manager")]
[Route("api/suppliers")]
public class SuppliersController : AuthenticatedControllerBase
{
    private readonly ISupplierManagementService _supplierService;

    public SuppliersController(ISupplierManagementService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSuppliers([FromQuery] string? keyword)
    {
        var result = await _supplierService.GetSuppliersAsync(keyword);
        return Ok(new ApiResponse<SupplierListResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] ManageSupplierRequest request)
    {
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _supplierService.AddAsync(request);
        return Ok(new ApiResponse<SupplierResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] ManageSupplierRequest request)
    {
        request.SupplierId = id;
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _supplierService.UpdateAsync(request);
        return Ok(new ApiResponse<SupplierResponse> { Success = true, Message = result.Message, Data = result });
    }
}
