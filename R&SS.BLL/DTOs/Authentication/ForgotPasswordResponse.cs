namespace R_SS.BLL.DTOs.Authentication;

public class ForgotPasswordResponse
{
    public string Message { get; set; } = string.Empty;

    public DateTime OtpExpiresAtUtc { get; set; }
}
