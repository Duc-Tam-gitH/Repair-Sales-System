using R_SS.BLL.DTOs.Configuration;

namespace R_SS.BLL.Interfaces;

public interface ISystemConfigurationService
{
    Task<IReadOnlyCollection<SystemConfigurationResponse>> GetByGroupAsync(string? groupName, string actorRole);
    Task<SystemConfigurationResponse> UpdateAsync(UpdateSystemConfigurationRequest request);
}
