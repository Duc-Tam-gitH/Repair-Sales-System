using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class SystemConfigurationRp : GenericRp<SystemConfiguration>, ISystemConfigurationRp
    {
        public SystemConfigurationRp(AppDbContext context) : base(context) { }
        public async Task<SystemConfiguration?> GetByKeyAsync(string key) => await _context.SystemConfigurations.FirstOrDefaultAsync(x => x.ConfigurationKey == key);
        public async Task<IReadOnlyCollection<SystemConfiguration>> GetByGroupAsync(string? groupName)
        {
            var query = _context.SystemConfigurations.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(groupName)) query = query.Where(x => x.GroupName == groupName);
            return await query.OrderBy(x => x.GroupName).ThenBy(x => x.ConfigurationKey).ToListAsync();
        }
    }
}
