using R_SS.BLL.DTOs.Product;

namespace R_SS.BLL.Interfaces;

public interface IProductService
{
    /// <summary>
    /// Gets active products currently managed by the system.
    /// </summary>
    Task<ProductListResponse> GetProductsAsync();

    /// <summary>
    /// Searches active products by name, category, brand, or all supported criteria.
    /// </summary>
    Task<ProductListResponse> SearchAsync(SearchProductsRequest request);

    /// <summary>
    /// Gets detailed information for one active product.
    /// </summary>
    Task<ProductDetailResponse> GetProductDetailsAsync(int productId);
}
