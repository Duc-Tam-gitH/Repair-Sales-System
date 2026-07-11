using FluentValidation;
using FluentValidation.Results;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Configuration;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class SystemConfigurationService : ISystemConfigurationService
{
    private static readonly Dictionary<string, (int Min, int Max)> IntegerRanges = new(StringComparer.OrdinalIgnoreCase)
    {
        ["OtpValidityMinutes"] = (1, 60),
        ["MaxOtpSendAttemptsPerHour"] = (1, 20),
        ["OtpIncorrectEntriesBeforeLockout"] = (1, 10),
        ["TemporaryLockoutMinutes"] = (1, 1440),
        ["MinimumStockLevelAlert"] = (0, 100000),
        ["DeliveryExpirationDays"] = (1, 30)
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemActivityLogService _activityLogService;
    private readonly IValidator<UpdateSystemConfigurationRequest> _validator;

    public SystemConfigurationService(
        IUnitOfWork unitOfWork,
        ISystemActivityLogService activityLogService,
        IValidator<UpdateSystemConfigurationRequest> validator)
    {
        _unitOfWork = unitOfWork;
        _activityLogService = activityLogService;
        _validator = validator;
    }

    public async Task<IReadOnlyCollection<SystemConfigurationResponse>> GetByGroupAsync(string? groupName, string actorRole)
    {
        EnsureAdmin(actorRole);
        var configurations = await _unitOfWork.SystemConfigurations.GetByGroupAsync(groupName);
        return configurations.Select(configuration => Map(configuration, string.Empty)).ToArray();
    }

    public async Task<SystemConfigurationResponse> UpdateAsync(UpdateSystemConfigurationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _validator.ValidateAsync(request));
        EnsureAdmin(request.ActorRole);
        ValidateKnownConfiguration(request.Key, request.Value);

        var configuration = await _unitOfWork.SystemConfigurations.GetByKeyAsync(request.Key.Trim());
        if (configuration is null)
        {
            configuration = new SystemConfiguration
            {
                ConfigurationKey = request.Key.Trim(),
                ConfigurationValue = request.Value.Trim(),
                GroupName = string.IsNullOrWhiteSpace(request.GroupName) ? "General" : request.GroupName.Trim(),
                UpdatedByUserId = request.ActorUserId,
                UpdatedAtUtc = DateTime.UtcNow
            };
            await _unitOfWork.SystemConfigurations.AddAsync(configuration);
        }
        else
        {
            configuration.ConfigurationValue = request.Value.Trim();
            configuration.GroupName = string.IsNullOrWhiteSpace(request.GroupName) ? configuration.GroupName : request.GroupName.Trim();
            configuration.UpdatedByUserId = request.ActorUserId;
            configuration.UpdatedAtUtc = DateTime.UtcNow;
            _unitOfWork.SystemConfigurations.Update(configuration);
        }

        await _unitOfWork.SaveChangesAsync();
        await _activityLogService.LogAsync(request.ActorUserId, null, "System Configuration", "Update", configuration.ConfigurationKey, "Success");
        return Map(configuration, "System configuration updated successfully.");
    }

    private static void ValidateKnownConfiguration(string key, string value)
    {
        if (!IntegerRanges.TryGetValue(key.Trim(), out var range))
        {
            return;
        }

        if (!int.TryParse(value.Trim(), out var parsed) || parsed < range.Min || parsed > range.Max)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(UpdateSystemConfigurationRequest.Value), $"{key} must be between {range.Min} and {range.Max}.")
            });
        }
    }

    private static SystemConfigurationResponse Map(SystemConfiguration configuration, string message) => new()
    {
        SystemConfigurationId = configuration.SystemConfigurationId,
        Key = configuration.ConfigurationKey,
        Value = configuration.ConfigurationValue,
        GroupName = configuration.GroupName,
        Message = message
    };

    private static void EnsureAdmin(string role)
    {
        if (!role.Equals(RoleConstants.Admin, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Admin can manage system configuration.");
        }
    }

    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
