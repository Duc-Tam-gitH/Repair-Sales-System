namespace R_SS.BLL.DTOs.Notification;

public class PreviewNotificationTemplateRequest
{
    public string TemplateCode { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ActorRole { get; set; } = string.Empty;
    public IDictionary<string, string> SampleData { get; set; } = new Dictionary<string, string>();
}
