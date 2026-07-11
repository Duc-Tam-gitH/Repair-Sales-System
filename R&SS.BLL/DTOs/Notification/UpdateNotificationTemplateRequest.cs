namespace R_SS.BLL.DTOs.Notification;

public class UpdateNotificationTemplateRequest
{
    public string TemplateCode { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Content { get; set; } = string.Empty;
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
}
