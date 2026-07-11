using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class ProductManagementHistoryRp : GenericRp<ProductManagementHistory>, IProductManagementHistoryRp
    {
        public ProductManagementHistoryRp(AppDbContext context) : base(context)
        {
        }
    }
}
