namespace R_SS.BLL.DTOs.Authentication;

public class ChangePasswordResponse
{
    public string Message { get; set; } = string.Empty;
    public DateTime ChangedAtUtc { get; set; }
}
