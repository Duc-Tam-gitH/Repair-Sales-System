using FluentValidation;
using FluentValidation.Results;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.RolePermission;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class RolePermissionService : IRolePermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemActivityLogService _activityLogService;
    private readonly IValidator<ManageRoleRequest> _manageValidator;
    private readonly IValidator<AssignRolePermissionsRequest> _assignValidator;
    private readonly IValidator<DeleteRoleRequest> _deleteValidator;

    public RolePermissionService(
        IUnitOfWork unitOfWork,
        ISystemActivityLogService activityLogService,
        IValidator<ManageRoleRequest> manageValidator,
        IValidator<AssignRolePermissionsRequest> assignValidator,
        IValidator<DeleteRoleRequest> deleteValidator)
    {
        _unitOfWork = unitOfWork;
        _activityLogService = activityLogService;
        _manageValidator = manageValidator;
        _assignValidator = assignValidator;
        _deleteValidator = deleteValidator;
    }

    public async Task<RoleResponse> CreateOrUpdateRoleAsync(ManageRoleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _manageValidator.ValidateAsync(request));
        EnsureAdmin(request.ActorRole);

        if (await _unitOfWork.Roles.ExistsNameAsync(request.RoleName.Trim(), request.RoleId))
        {
            throw new InvalidOperationException("Role name already exists.");
        }

        Role role;
        if (request.RoleId.HasValue)
        {
            role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId.Value)
                ?? throw new NotFoundException("Role not found.");
            role.RoleName = request.RoleName.Trim();
            role.Description = Normalize(request.Description);
            _unitOfWork.Roles.Update(role);
        }
        else
        {
            role = new Role { RoleName = request.RoleName.Trim(), Description = Normalize(request.Description) };
            await _unitOfWork.Roles.AddAsync(role);
        }

        await _unitOfWork.SaveChangesAsync();
        await _activityLogService.LogAsync(request.ActorUserId, null, "Role and Permission Management", request.RoleId.HasValue ? "Update Role" : "Create Role", role.RoleName, "Success");
        return await MapAsync(role, request.RoleId.HasValue ? "Role updated successfully." : "Role created successfully.");
    }

    public async Task<RoleResponse> AssignPermissionsAsync(AssignRolePermissionsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _assignValidator.ValidateAsync(request));
        EnsureAdmin(request.ActorRole);

        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId)
            ?? throw new NotFoundException("Role not found.");

        var permissionCodes = request.PermissionCodes
            .Select(permission => permission.Trim())
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (role.RoleName.Equals(RoleConstants.Admin, StringComparison.OrdinalIgnoreCase) && permissionCodes.Length == 0)
        {
            throw new InvalidOperationException("Admin role must keep at least one permission.");
        }

        var existingPermissions = await _unitOfWork.RolePermissions.GetByRoleIdAsync(role.RoleId);
        _unitOfWork.RolePermissions.DeleteRange(existingPermissions);
        foreach (var permissionCode in permissionCodes)
        {
            await _unitOfWork.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = role.RoleId,
                Role = role,
                UseCaseId = permissionCode,
                FunctionName = permissionCode
            });
        }

        await _unitOfWork.SaveChangesAsync();
        await _activityLogService.LogAsync(request.ActorUserId, null, "Role and Permission Management", "Assign Permissions", role.RoleName, "Success");
        return await MapAsync(role, "Permissions assigned successfully.");
    }

    public async Task DeleteRoleAsync(DeleteRoleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _deleteValidator.ValidateAsync(request));
        EnsureAdmin(request.ActorRole);

        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId)
            ?? throw new NotFoundException("Role not found.");

        if (role.RoleName.Equals(RoleConstants.Admin, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Admin role cannot be deleted.");
        }

        if (await _unitOfWork.Roles.HasUsersAsync(role.RoleId))
        {
            throw new InvalidOperationException("Role cannot be deleted because users are assigned to it.");
        }

        var permissions = await _unitOfWork.RolePermissions.GetByRoleIdAsync(role.RoleId);
        _unitOfWork.RolePermissions.DeleteRange(permissions);
        _unitOfWork.Roles.Delete(role);
        await _unitOfWork.SaveChangesAsync();
        await _activityLogService.LogAsync(request.ActorUserId, null, "Role and Permission Management", "Delete Role", role.RoleName, "Success");
    }

    private async Task<RoleResponse> MapAsync(Role role, string message)
    {
        var permissions = await _unitOfWork.RolePermissions.GetByRoleIdAsync(role.RoleId);
        return new RoleResponse
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Description = role.Description,
            PermissionCodes = permissions.Select(permission => permission.UseCaseId).ToArray(),
            Message = message
        };
    }

    private static void EnsureAdmin(string role)
    {
        if (!role.Equals(RoleConstants.Admin, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Admin can manage roles and permissions.");
        }
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
