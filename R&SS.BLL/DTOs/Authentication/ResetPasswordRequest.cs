namespace R_SS.BLL.DTOs.Authentication;

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;
}
