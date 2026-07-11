using R_SS.BLL.DTOs.Activity;

namespace R_SS.BLL.Interfaces;

public interface ISystemActivityLogService
{
    Task LogAsync(int? actorUserId, string? actorUsername, string functionName, string operationType, string? affectedData, string result, string? details = null);
    Task<SystemActivityLogListResponse> SearchAsync(SystemActivityLogSearchRequest request);
    Task<SystemActivityLogResponse> GetDetailsAsync(int logId, string actorRole);
}
