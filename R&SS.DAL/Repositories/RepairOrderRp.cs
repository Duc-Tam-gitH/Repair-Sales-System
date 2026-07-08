using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class RepairOrderRp : GenericRp<RepairOrder>, IRepairOrderRp
    {
        public RepairOrderRp(AppDbContext context) : base(context)
        {
        }
    }
}
