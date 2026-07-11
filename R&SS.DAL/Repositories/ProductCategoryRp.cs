using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class ProductCategoryRp : GenericRp<ProductCategory>, IProductCategoryRp
    {
        public ProductCategoryRp(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsNameAsync(string categoryName, int? excludedCategoryId = null)
        {
            return await _context.ProductCategories.AnyAsync(category =>
                category.CategoryName.ToLower() == categoryName.ToLower() &&
                (!excludedCategoryId.HasValue || category.ProductCategoryId != excludedCategoryId.Value));
        }

        public async Task<bool> ExistsCodeAsync(string categoryCode, int? excludedCategoryId = null)
        {
            return await _context.ProductCategories.AnyAsync(category =>
                category.CategoryCode.ToLower() == categoryCode.ToLower() &&
                (!excludedCategoryId.HasValue || category.ProductCategoryId != excludedCategoryId.Value));
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products.AnyAsync(product => product.ProductCategoryId == categoryId);
        }
    }
}
