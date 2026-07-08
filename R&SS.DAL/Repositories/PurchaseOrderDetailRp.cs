using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class PurchaseOrderDetailRp : GenericRp<PurchaseOrderDetail>, IPurchaseOrderDetailRp
    {
        public PurchaseOrderDetailRp(AppDbContext context) : base(context)
        {
        }
    }
}
