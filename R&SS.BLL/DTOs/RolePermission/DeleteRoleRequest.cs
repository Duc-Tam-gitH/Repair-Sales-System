namespace R_SS.BLL.DTOs.RolePermission;

public class DeleteRoleRequest
{
    public int RoleId { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
}
