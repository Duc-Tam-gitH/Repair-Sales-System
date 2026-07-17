using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Product;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : AuthenticatedControllerBase
{
    private readonly IProductService _productService;
    private readonly IProductManagementService _productManagementService;

    public ProductsController(IProductService productService, IProductManagementService productManagementService)
    {
        _productService = productService;
        _productManagementService = productManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var result = await _productService.GetProductsAsync();
        return Ok(new ApiResponse<ProductListResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string keyword, [FromQuery] string criteria = "all")
    {
        var result = await _productService.SearchAsync(new SearchProductsRequest { Keyword = keyword, Criteria = criteria });
        return Ok(new ApiResponse<ProductListResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var result = await _productService.GetProductDetailsAsync(id);
        return Ok(new ApiResponse<ProductDetailResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Manager")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ManageProductRequest request)
    {
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _productManagementService.AddAsync(request);
        return Ok(new ApiResponse<ProductManagementResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Manager")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ManageProductRequest request)
    {
        request.ProductId = id;
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _productManagementService.UpdateAsync(request);
        return Ok(new ApiResponse<ProductManagementResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Manager")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productManagementService.DeleteAsync(id, CurrentUserId(), CurrentRole());
        return Ok(new ApiResponse<ProductManagementResponse> { Success = true, Message = result.Message, Data = result });
    }
}
