using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IRolePermissionRp : IGenericRp<RolePermission>
    {
        Task<IReadOnlyCollection<RolePermission>> GetByRoleIdAsync(int roleId);
        void DeleteRange(IEnumerable<RolePermission> permissions);
    }
}
