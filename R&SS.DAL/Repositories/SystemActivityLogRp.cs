using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class SystemActivityLogRp : GenericRp<SystemActivityLog>, ISystemActivityLogRp
    {
        public SystemActivityLogRp(AppDbContext context) : base(context) { }

        public async Task<IReadOnlyCollection<SystemActivityLog>> SearchAsync(DateTime? fromUtc, DateTime? toUtc, int? actorUserId, string? functionName, string? operationType)
        {
            var query = _context.SystemActivityLogs.AsNoTracking().AsQueryable();
            if (fromUtc.HasValue) query = query.Where(log => log.ExecutedAtUtc >= fromUtc.Value);
            if (toUtc.HasValue) query = query.Where(log => log.ExecutedAtUtc <= toUtc.Value);
            if (actorUserId.HasValue) query = query.Where(log => log.ActorUserId == actorUserId.Value);
            if (!string.IsNullOrWhiteSpace(functionName)) query = query.Where(log => log.FunctionName.ToLower().Contains(functionName.ToLower()));
            if (!string.IsNullOrWhiteSpace(operationType)) query = query.Where(log => log.OperationType.ToLower() == operationType.ToLower());
            return await query.OrderByDescending(log => log.ExecutedAtUtc).ToListAsync();
        }
    }
}
