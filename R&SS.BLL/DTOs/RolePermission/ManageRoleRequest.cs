namespace R_SS.BLL.DTOs.RolePermission;

public class ManageRoleRequest
{
    public int? RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
}
