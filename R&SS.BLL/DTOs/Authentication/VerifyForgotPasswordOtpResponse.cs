namespace R_SS.BLL.DTOs.Authentication;

public class VerifyForgotPasswordOtpResponse
{
    public string Message { get; set; } = string.Empty;

    public DateTime VerifiedAtUtc { get; set; }
}
