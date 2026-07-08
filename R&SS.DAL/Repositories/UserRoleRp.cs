using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class UserRoleRp : GenericRp<UserRole>, IUserRoleRp
    {
        public UserRoleRp(AppDbContext context) : base(context)
        {
        }
    }
}
