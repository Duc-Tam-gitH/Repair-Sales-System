using R_SS.BLL.DTOs.RolePermission;

namespace R_SS.BLL.Interfaces;

public interface IRolePermissionService
{
    Task<RoleResponse> CreateOrUpdateRoleAsync(ManageRoleRequest request);
    Task<RoleResponse> AssignPermissionsAsync(AssignRolePermissionsRequest request);
    Task DeleteRoleAsync(DeleteRoleRequest request);
}
