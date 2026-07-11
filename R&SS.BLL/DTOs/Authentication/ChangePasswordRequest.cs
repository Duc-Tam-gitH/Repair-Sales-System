namespace R_SS.BLL.DTOs.Authentication;

public class ChangePasswordRequest
{
    public int UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
