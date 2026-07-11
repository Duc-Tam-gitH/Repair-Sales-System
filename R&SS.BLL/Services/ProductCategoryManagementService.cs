using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.ProductCategory;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class ProductCategoryManagementService : IProductCategoryManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<ManageProductCategoryRequest> _validator;
    private readonly ILogger<ProductCategoryManagementService> _logger;

    public ProductCategoryManagementService(IUnitOfWork unitOfWork, IValidator<ManageProductCategoryRequest> validator, ILogger<ProductCategoryManagementService> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ProductCategoryResponse> AddAsync(ManageProductCategoryRequest request)
    {
        await ValidateAsync(request);
        if (await _unitOfWork.ProductCategories.ExistsNameAsync(request.CategoryName))
        {
            throw new InvalidOperationException("Category name already exists.");
        }

        var category = new ProductCategory
        {
            CategoryCode = CreateCategoryCode(),
            CategoryName = request.CategoryName.Trim(),
            Description = Normalize(request.Description),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _unitOfWork.ProductCategories.AddAsync(category);
        await AddHistoryAsync(category, request.ActorUserId, "Add", "Category added.");
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Manager {UserId} added category {CategoryName}.", request.ActorUserId, category.CategoryName);
        return Map(category, "Category added successfully.");
    }

    public async Task<ProductCategoryResponse> UpdateAsync(ManageProductCategoryRequest request)
    {
        await ValidateAsync(request);
        if (!request.ProductCategoryId.HasValue)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.ProductCategoryId), "Category id is required.") });
        }

        var category = await _unitOfWork.ProductCategories.GetByIdAsync(request.ProductCategoryId.Value);
        if (category is null) throw new NotFoundException("Category not found.");
        if (await _unitOfWork.ProductCategories.ExistsNameAsync(request.CategoryName, category.ProductCategoryId))
        {
            throw new InvalidOperationException("Category name already exists.");
        }

        category.CategoryName = request.CategoryName.Trim();
        category.Description = Normalize(request.Description);
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ProductCategories.Update(category);
        await AddHistoryAsync(category, request.ActorUserId, "Update", "Category updated.");
        await _unitOfWork.SaveChangesAsync();
        return Map(category, "Category updated successfully.");
    }

    public async Task<ProductCategoryResponse> DeleteAsync(int categoryId, int actorUserId, string actorRole)
    {
        EnsureManager(actorRole);
        if (await _unitOfWork.ProductCategories.HasProductsAsync(categoryId))
        {
            throw new InvalidOperationException("Category cannot be deleted because it is linked to products.");
        }

        var category = await _unitOfWork.ProductCategories.GetByIdAsync(categoryId);
        if (category is null) throw new NotFoundException("Category not found.");
        _unitOfWork.ProductCategories.Delete(category);
        await AddHistoryAsync(category, actorUserId, "Delete", "Category deleted.");
        await _unitOfWork.SaveChangesAsync();
        return Map(category, "Category deleted successfully.");
    }

    private async Task ValidateAsync(ManageProductCategoryRequest request)
    {
        ThrowIfInvalid(await _validator.ValidateAsync(request));
        EnsureManager(request.ActorRole);
    }

    private async Task AddHistoryAsync(ProductCategory category, int actorUserId, string operation, string content)
    {
        await _unitOfWork.ProductCategoryManagementHistories.AddAsync(new ProductCategoryManagementHistory
        {
            ProductCategory = category,
            ProductCategoryId = category.ProductCategoryId,
            ActorUserId = actorUserId,
            Operation = operation,
            ChangedContent = content,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    private static ProductCategoryResponse Map(ProductCategory category, string message) => new()
    {
        ProductCategoryId = category.ProductCategoryId,
        CategoryCode = category.CategoryCode,
        CategoryName = category.CategoryName,
        Description = category.Description,
        IsActive = category.IsActive,
        Message = message
    };

    private static string CreateCategoryCode() => $"CAT-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void EnsureManager(string role)
    {
        if (!role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedException("Only Managers can manage product categories.");
    }
    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid) throw new ValidationException(result.Errors);
    }
}
