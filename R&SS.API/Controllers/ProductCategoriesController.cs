using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.ProductCategory;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize(Roles = "Manager")]
[Route("api/product-categories")]
[Route("api/categories")]
public class ProductCategoriesController : AuthenticatedControllerBase
{
    private readonly IProductCategoryManagementService _categoryService;
    private readonly IUnitOfWork _unitOfWork;

    public ProductCategoriesController(IProductCategoryManagementService categoryService, IUnitOfWork unitOfWork)
    {
        _categoryService = categoryService;
        _unitOfWork = unitOfWork;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _unitOfWork.ProductCategories.GetAllAsync();
        var data = categories
            .OrderBy(category => category.CategoryName)
            .Select(category => new ProductCategoryResponse
            {
                ProductCategoryId = category.ProductCategoryId,
                CategoryCode = category.CategoryCode,
                CategoryName = category.CategoryName,
                Description = category.Description,
                IsActive = category.IsActive,
                Message = string.Empty
            })
            .ToArray();

        return Ok(new ApiResponse<IReadOnlyCollection<ProductCategoryResponse>>
        {
            Success = true,
            Message = data.Length == 0 ? "No categories found." : "Categories retrieved successfully.",
            Data = data
        });
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
