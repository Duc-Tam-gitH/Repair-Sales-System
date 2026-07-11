using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface ISystemConfigurationRp : IGenericRp<SystemConfiguration>
    {
        Task<SystemConfiguration?> GetByKeyAsync(string key);
        Task<IReadOnlyCollection<SystemConfiguration>> GetByGroupAsync(string? groupName);
    }
}
