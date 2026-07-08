using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class CustomerRp : GenericRp<Customer>, ICustomerRp
    {
        public CustomerRp(AppDbContext context) : base(context)
        {
        }
    }
}
