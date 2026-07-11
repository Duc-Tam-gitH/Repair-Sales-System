using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class NotificationTemplateRp : GenericRp<NotificationTemplate>, INotificationTemplateRp
    {
        public NotificationTemplateRp(AppDbContext context) : base(context) { }

        public async Task<NotificationTemplate?> GetByCodeAsync(string templateCode)
        {
            return await _context.NotificationTemplates.FirstOrDefaultAsync(x => x.TemplateCode == templateCode);
        }

        public async Task<IReadOnlyCollection<NotificationTemplate>> GetWithDefaultsAsync()
        {
            return await _context.NotificationTemplates.AsNoTracking().OrderBy(x => x.TemplateCode).ToListAsync();
        }
    }
}
