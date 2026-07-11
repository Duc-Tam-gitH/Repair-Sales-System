using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IProductRp : IGenericRp<Product>
    {
        Task<IReadOnlyCollection<Product>> GetActiveProductsAsync();
        Task<IReadOnlyCollection<Product>> SearchActiveProductsAsync(string keyword, string criteria);
        Task<Product?> GetActiveProductByIdAsync(int productId);
        Task<bool> ExistsCodeAsync(string productCode, int? excludedProductId = null);
        Task<bool> HasReferencesAsync(int productId);
    }
}
