namespace R_SS.BLL.DTOs.Notification;

public class NotificationTemplatePreviewResponse
{
    public string TemplateCode { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
