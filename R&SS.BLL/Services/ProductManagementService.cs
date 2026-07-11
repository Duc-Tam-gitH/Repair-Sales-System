using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Product;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class ProductManagementService : IProductManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<ManageProductRequest> _validator;
    private readonly ILogger<ProductManagementService> _logger;

    public ProductManagementService(IUnitOfWork unitOfWork, IValidator<ManageProductRequest> validator, ILogger<ProductManagementService> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ProductManagementResponse> AddAsync(ManageProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        await ValidateRequestAsync(request);
        if (await _unitOfWork.Products.ExistsCodeAsync(request.ProductCode))
        {
            throw new InvalidOperationException("Product code already exists.");
        }

        var product = new Product();
        Apply(product, request);
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Products.AddAsync(product);
        await AddHistoryAsync(product, request.ActorUserId, "Add", "Product added.");
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Manager {UserId} added product {ProductCode}.", request.ActorUserId, product.ProductCode);
        return Map(product, "Product added successfully.");
    }

    public async Task<ProductManagementResponse> UpdateAsync(ManageProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        await ValidateRequestAsync(request);
        if (!request.ProductId.HasValue)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.ProductId), "Product id is required.") });
        }

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId.Value);
        if (product is null)
        {
            throw new NotFoundException("Product not found.");
        }

        if (await _unitOfWork.Products.ExistsCodeAsync(request.ProductCode, product.ProductId))
        {
            throw new InvalidOperationException("Product code already exists.");
        }

        Apply(product, request);
        product.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Products.Update(product);
        await AddHistoryAsync(product, request.ActorUserId, "Update", "Product updated.");
        await _unitOfWork.SaveChangesAsync();
        return Map(product, "Product updated successfully.");
    }

    public async Task<ProductManagementResponse> DeleteAsync(int productId, int actorUserId, string actorRole)
    {
        EnsureManager(actorRole);
        if (await _unitOfWork.Products.HasReferencesAsync(productId))
        {
            throw new InvalidOperationException("Product cannot be deleted because it is referenced by orders or repair tickets.");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product is null)
        {
            throw new NotFoundException("Product not found.");
        }

        _unitOfWork.Products.Delete(product);
        await AddHistoryAsync(product, actorUserId, "Delete", "Product deleted.");
        await _unitOfWork.SaveChangesAsync();
        return Map(product, "Product deleted successfully.");
    }

    private async Task ValidateRequestAsync(ManageProductRequest request)
    {
        ThrowIfInvalid(await _validator.ValidateAsync(request));
        EnsureManager(request.ActorRole);
    }

    private static void Apply(Product product, ManageProductRequest request)
    {
        product.ProductCode = request.ProductCode.Trim();
        product.ProductName = request.ProductName.Trim();
        product.ProductCategoryId = request.ProductCategoryId;
        product.SupplierId = request.SupplierId;
        product.SalePrice = request.SalePrice;
        product.QuantityInStock = request.QuantityInStock;
        product.Description = Normalize(request.Description);
        product.ImageUrl = Normalize(request.ImageUrl);
        product.IsActive = true;
    }

    private async Task AddHistoryAsync(Product product, int actorUserId, string operation, string content)
    {
        await _unitOfWork.ProductManagementHistories.AddAsync(new ProductManagementHistory
        {
            Product = product,
            ProductId = product.ProductId,
            ActorUserId = actorUserId,
            Operation = operation,
            ChangedContent = content,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    private static ProductManagementResponse Map(Product product, string message) => new()
    {
        ProductId = product.ProductId,
        ProductCode = product.ProductCode,
        ProductName = product.ProductName,
        SalePrice = product.SalePrice,
        QuantityInStock = product.QuantityInStock,
        Message = message
    };

    private static void EnsureManager(string role)
    {
        if (!role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Managers can manage products.");
        }
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid) throw new ValidationException(result.Errors);
    }
}
