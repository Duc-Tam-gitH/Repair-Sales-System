namespace R_SS.BLL.DTOs.Authentication;

public class ResetPasswordResponse
{
    public string Message { get; set; } = string.Empty;

    public DateTime ResetAtUtc { get; set; }
}
