using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface INotificationTemplateHistoryRp : IGenericRp<NotificationTemplateHistory>
    {
        Task<IReadOnlyCollection<NotificationTemplateHistory>> GetByTemplateIdAsync(int templateId);
    }
}
