using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.ProductCategory;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize(Roles = "Manager")]
[Route("api/product-categories")]
public class ProductCategoriesController : AuthenticatedControllerBase
{
    private readonly IProductCategoryManagementService _categoryService;

    public ProductCategoriesController(IProductCategoryManagementService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] ManageProductCategoryRequest request)
    {
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _categoryService.AddAsync(request);
        return Ok(new ApiResponse<ProductCategoryResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] ManageProductCategoryRequest request)
    {
        request.ProductCategoryId = id;
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _categoryService.UpdateAsync(request);
        return Ok(new ApiResponse<ProductCategoryResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryService.DeleteAsync(id, CurrentUserId(), CurrentRole());
        return Ok(new ApiResponse<ProductCategoryResponse> { Success = true, Message = result.Message, Data = result });
    }
}
