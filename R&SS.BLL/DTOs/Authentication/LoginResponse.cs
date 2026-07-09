namespace R_SS.BLL.DTOs.Authentication;

public class LoginResponse
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public string Message { get; set; } = string.Empty;
}
