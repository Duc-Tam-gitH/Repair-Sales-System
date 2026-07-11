namespace R_SS.BLL.DTOs.RolePermission;

public class RoleResponse
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyCollection<string> PermissionCodes { get; set; } = Array.Empty<string>();
    public string Message { get; set; } = string.Empty;
}
