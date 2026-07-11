using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface ISystemActivityLogRp : IGenericRp<SystemActivityLog>
    {
        Task<IReadOnlyCollection<SystemActivityLog>> SearchAsync(DateTime? fromUtc, DateTime? toUtc, int? actorUserId, string? functionName, string? operationType);
    }
}
