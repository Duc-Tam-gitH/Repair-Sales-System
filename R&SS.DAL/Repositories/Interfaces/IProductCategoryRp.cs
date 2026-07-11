using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IProductCategoryRp : IGenericRp<ProductCategory>
    {
        Task<bool> ExistsNameAsync(string categoryName, int? excludedCategoryId = null);
        Task<bool> ExistsCodeAsync(string categoryCode, int? excludedCategoryId = null);
        Task<bool> HasProductsAsync(int categoryId);
    }
}
