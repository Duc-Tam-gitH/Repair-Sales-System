using R_SS.BLL.DTOs.Notification;

namespace R_SS.BLL.Interfaces;

public interface INotificationTemplateService
{
    Task<IReadOnlyCollection<NotificationTemplateResponse>> GetTemplatesAsync(string actorRole);
    Task<NotificationTemplateResponse> UpdateAsync(UpdateNotificationTemplateRequest request);
    Task<NotificationTemplatePreviewResponse> PreviewAsync(PreviewNotificationTemplateRequest request);
    Task<NotificationTemplateResponse> RestoreDefaultAsync(RestoreNotificationTemplateRequest request);
}
