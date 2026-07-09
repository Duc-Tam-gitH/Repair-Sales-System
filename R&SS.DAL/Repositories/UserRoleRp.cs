using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace R_SS.DAL.Repositories
{
    public class UserRoleRp : GenericRp<UserRole>, IUserRoleRp
    {
        public UserRoleRp(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<UserRole>> GetByUserIdAsync(int userId)
        {
            return await _context.UserRoles
                .Include(userRole => userRole.Role)
                .Where(userRole => userRole.UserId == userId)
                .OrderBy(userRole => userRole.UserRoleId)
                .ToListAsync();
        }
    }
}
