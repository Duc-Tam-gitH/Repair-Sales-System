namespace R_SS.BLL.DTOs.Authentication;

public class PersonalInfoResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string Message { get; set; } = string.Empty;
}
