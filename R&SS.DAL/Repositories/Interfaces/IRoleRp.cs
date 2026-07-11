using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IRoleRp : IGenericRp<Role>
    {
        Task<bool> ExistsNameAsync(string roleName, int? excludedRoleId = null);
        Task<bool> HasUsersAsync(int roleId);
    }
}
