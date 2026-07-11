using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.DTOs.Product;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;

namespace R_SS.BLL.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<SearchProductsRequest> _searchValidator;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<SearchProductsRequest> searchValidator,
        ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _searchValidator = searchValidator;
        _logger = logger;
    }

    /// <summary>
    /// Gets active products currently managed by the system.
    /// </summary>
    public async Task<ProductListResponse> GetProductsAsync()
    {
        var products = await _unitOfWork.Products.GetActiveProductsAsync();
        var mappedProducts = _mapper.Map<IReadOnlyCollection<ProductResponse>>(products);
        _logger.LogInformation("Retrieved {ProductCount} active products.", mappedProducts.Count);

        return new ProductListResponse
        {
            Products = mappedProducts,
            Message = mappedProducts.Count == 0 ? "No products available." : "Products retrieved successfully."
        };
    }

    /// <summary>
    /// Searches active products by name, category, brand, or all supported criteria.
    /// </summary>
    public async Task<ProductListResponse> SearchAsync(SearchProductsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _searchValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);

        var products = await _unitOfWork.Products.SearchActiveProductsAsync(request.Keyword, request.Criteria);
        var mappedProducts = _mapper.Map<IReadOnlyCollection<ProductResponse>>(products);
        _logger.LogInformation("Product search returned {ProductCount} results.", mappedProducts.Count);

        return new ProductListResponse
        {
            Products = mappedProducts,
            Message = mappedProducts.Count == 0 ? "No matching products found." : "Products retrieved successfully."
        };
    }

    /// <summary>
    /// Gets detailed information for one active product.
    /// </summary>
    public async Task<ProductDetailResponse> GetProductDetailsAsync(int productId)
    {
        if (productId <= 0)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(productId), "Product id must be greater than 0.") });
        }

        var product = await _unitOfWork.Products.GetActiveProductByIdAsync(productId);
        if (product is null)
        {
            throw new NotFoundException("Product not found.");
        }

        var response = _mapper.Map<ProductDetailResponse>(product);
        response.Message = "Product details retrieved successfully.";
        return response;
    }

    private static void ThrowIfInvalid(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
