using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface INotificationTemplateRp : IGenericRp<NotificationTemplate>
    {
        Task<NotificationTemplate?> GetByCodeAsync(string templateCode);
        Task<IReadOnlyCollection<NotificationTemplate>> GetWithDefaultsAsync();
    }
}
