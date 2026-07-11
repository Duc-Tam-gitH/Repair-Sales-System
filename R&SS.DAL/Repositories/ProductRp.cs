using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class ProductRp : GenericRp<Product>, IProductRp
    {
        public ProductRp(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<Product>> GetActiveProductsAsync()
        {
            return await BuildActiveProductQuery()
                .OrderBy(product => product.ProductName)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<Product>> SearchActiveProductsAsync(string keyword, string criteria)
        {
            var normalizedKeyword = keyword.Trim().ToLower();
            var normalizedCriteria = criteria.Trim().ToLower();
            var query = BuildActiveProductQuery();

            query = normalizedCriteria switch
            {
                "name" => query.Where(product => product.ProductName.ToLower().Contains(normalizedKeyword)),
                "category" => query.Where(product =>
                    product.ProductCategory != null &&
                    product.ProductCategory.CategoryName.ToLower().Contains(normalizedKeyword)),
                "brand" => query.Where(product =>
                    product.Supplier != null &&
                    product.Supplier.SupplierName.ToLower().Contains(normalizedKeyword)),
                _ => query.Where(product =>
                    product.ProductName.ToLower().Contains(normalizedKeyword) ||
                    (product.ProductCategory != null && product.ProductCategory.CategoryName.ToLower().Contains(normalizedKeyword)) ||
                    (product.Supplier != null && product.Supplier.SupplierName.ToLower().Contains(normalizedKeyword))
                )
            };

            return await query
                .OrderBy(product => product.ProductName)
                .ToListAsync();
        }

        public async Task<Product?> GetActiveProductByIdAsync(int productId)
        {
            return await BuildActiveProductQuery()
                .FirstOrDefaultAsync(product => product.ProductId == productId);
        }

        public async Task<bool> ExistsCodeAsync(string productCode, int? excludedProductId = null)
        {
            return await _context.Products.AnyAsync(product =>
                product.ProductCode.ToLower() == productCode.ToLower() &&
                (!excludedProductId.HasValue || product.ProductId != excludedProductId.Value));
        }

        public async Task<bool> HasReferencesAsync(int productId)
        {
            return await _context.SalesOrderDetails.AnyAsync(detail => detail.ProductId == productId) ||
                await _context.RepairOrderDetails.AnyAsync(detail => detail.ProductId == productId);
        }

        private IQueryable<Product> BuildActiveProductQuery()
        {
            return _context.Products
                .AsNoTracking()
                .Include(product => product.ProductCategory)
                .Include(product => product.Supplier)
                .Where(product => product.IsActive);
        }
    }
}
