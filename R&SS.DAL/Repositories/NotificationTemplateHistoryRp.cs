using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class NotificationTemplateHistoryRp : GenericRp<NotificationTemplateHistory>, INotificationTemplateHistoryRp
    {
        public NotificationTemplateHistoryRp(AppDbContext context) : base(context) { }

        public async Task<IReadOnlyCollection<NotificationTemplateHistory>> GetByTemplateIdAsync(int templateId)
        {
            return await _context.NotificationTemplateHistories
                .AsNoTracking()
                .Where(x => x.NotificationTemplateId == templateId)
                .OrderByDescending(x => x.EditedAtUtc)
                .ToListAsync();
        }
    }
}
