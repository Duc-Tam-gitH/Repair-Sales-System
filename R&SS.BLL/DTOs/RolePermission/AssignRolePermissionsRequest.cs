namespace R_SS.BLL.DTOs.RolePermission;

public class AssignRolePermissionsRequest
{
    public int RoleId { get; set; }
    public IReadOnlyCollection<string> PermissionCodes { get; set; } = Array.Empty<string>();
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
}
