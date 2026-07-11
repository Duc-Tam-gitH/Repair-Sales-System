namespace R_SS.BLL.DTOs.Authentication;

public class VerifyForgotPasswordOtpRequest
{
    public string Email { get; set; } = string.Empty;

    public string OtpCode { get; set; } = string.Empty;
}
