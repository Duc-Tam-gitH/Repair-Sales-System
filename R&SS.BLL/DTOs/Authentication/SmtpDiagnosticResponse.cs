namespace R_SS.BLL.DTOs.Authentication;

public class SmtpDiagnosticResponse
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string SocketOption { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
