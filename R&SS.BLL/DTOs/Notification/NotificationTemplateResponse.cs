namespace R_SS.BLL.DTOs.Notification;

public class NotificationTemplateResponse
{
    public int NotificationTemplateId { get; set; }
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Content { get; set; } = string.Empty;
    public IReadOnlyCollection<string> AllowedVariables { get; set; } = Array.Empty<string>();
    public DateTime UpdatedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
