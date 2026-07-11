namespace R_SS.BLL.DTOs.Account;

public class ManageAccountRequest
{
    public int? UserId { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
