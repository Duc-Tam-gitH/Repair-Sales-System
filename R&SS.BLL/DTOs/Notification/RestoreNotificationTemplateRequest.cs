namespace R_SS.BLL.DTOs.Notification;

public class RestoreNotificationTemplateRequest
{
    public string TemplateCode { get; set; } = string.Empty;
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
}
