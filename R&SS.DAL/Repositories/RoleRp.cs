using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class RoleRp : GenericRp<Role>, IRoleRp
    {
        public RoleRp(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsNameAsync(string roleName, int? excludedRoleId = null)
        {
            return await _context.Roles.AnyAsync(role =>
                role.RoleName.ToLower() == roleName.ToLower() &&
                (!excludedRoleId.HasValue || role.RoleId != excludedRoleId.Value));
        }

        public async Task<bool> HasUsersAsync(int roleId)
        {
            return await _context.UserRoles.AnyAsync(userRole => userRole.RoleId == roleId);
        }
    }
}
