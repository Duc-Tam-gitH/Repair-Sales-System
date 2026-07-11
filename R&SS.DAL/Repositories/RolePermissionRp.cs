using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class RolePermissionRp : GenericRp<RolePermission>, IRolePermissionRp
    {
        public RolePermissionRp(AppDbContext context) : base(context) { }
        public async Task<IReadOnlyCollection<RolePermission>> GetByRoleIdAsync(int roleId) => await _context.RolePermissions.Where(x => x.RoleId == roleId).ToListAsync();
        public void DeleteRange(IEnumerable<RolePermission> permissions) => _context.RolePermissions.RemoveRange(permissions);
    }
}
