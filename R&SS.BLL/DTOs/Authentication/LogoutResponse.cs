namespace R_SS.BLL.DTOs.Authentication;

public class LogoutResponse
{
    public string Message { get; set; } = string.Empty;

    public DateTime LoggedOutAtUtc { get; set; }
}
