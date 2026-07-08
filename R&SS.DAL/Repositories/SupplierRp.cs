using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class SupplierRp : GenericRp<Supplier>, ISupplierRp
    {
        public SupplierRp(AppDbContext context) : base(context)
        {
        }
    }
}
