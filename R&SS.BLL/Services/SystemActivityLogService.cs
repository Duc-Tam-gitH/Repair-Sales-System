using FluentValidation;
using FluentValidation.Results;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Activity;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class SystemActivityLogService : ISystemActivityLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<SystemActivityLogSearchRequest> _validator;

    public SystemActivityLogService(IUnitOfWork unitOfWork, IValidator<SystemActivityLogSearchRequest> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task LogAsync(int? actorUserId, string? actorUsername, string functionName, string operationType, string? affectedData, string result, string? details = null)
    {
        await _unitOfWork.SystemActivityLogs.AddAsync(new SystemActivityLog
        {
            ActorUserId = actorUserId,
            ActorUsername = actorUsername,
            FunctionName = functionName,
            OperationType = operationType,
            AffectedData = affectedData,
            ExecutionResult = result,
            Details = details,
            ExecutedAtUtc = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<SystemActivityLogListResponse> SearchAsync(SystemActivityLogSearchRequest request)
    {
        ThrowIfInvalid(await _validator.ValidateAsync(request));
        EnsureManager(request.ActorRole);
        var logs = await _unitOfWork.SystemActivityLogs.SearchAsync(request.FromUtc, request.ToUtc, request.ActorUserId, request.FunctionName, request.OperationType);
        var mapped = logs.Select(Map).ToArray();
        await LogAsync(null, null, "System Activity History Management", "View", null, "Success");
        return new SystemActivityLogListResponse { Logs = mapped, Message = mapped.Length == 0 ? "No data was found." : "Activity history retrieved successfully." };
    }

    public async Task<SystemActivityLogResponse> GetDetailsAsync(int logId, string actorRole)
    {
        EnsureManager(actorRole);
        var log = await _unitOfWork.SystemActivityLogs.GetByIdAsync(logId);
        if (log is null) throw new NotFoundException("Activity history record not found.");
        return Map(log);
    }

    private static SystemActivityLogResponse Map(SystemActivityLog log) => new()
    {
        SystemActivityLogId = log.SystemActivityLogId,
        ActorUserId = log.ActorUserId,
        ActorUsername = log.ActorUsername,
        FunctionName = log.FunctionName,
        OperationType = log.OperationType,
        AffectedData = log.AffectedData,
        ExecutionResult = log.ExecutionResult,
        ExecutedAtUtc = log.ExecutedAtUtc,
        Details = log.Details
    };

    private static void EnsureManager(string role)
    {
        if (!role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedException("Only Managers can access activity history.");
    }
    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid) throw new ValidationException(result.Errors);
    }
}
