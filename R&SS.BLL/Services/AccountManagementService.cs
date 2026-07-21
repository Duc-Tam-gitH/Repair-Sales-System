using FluentValidation;
using FluentValidation.Results;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Account;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class AccountManagementService : IAccountManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISystemActivityLogService _activityLogService;
    private readonly IValidator<ManageAccountRequest> _validator;

    public AccountManagementService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ISystemActivityLogService activityLogService, IValidator<ManageAccountRequest> validator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _activityLogService = activityLogService;
        _validator = validator;
    }

    public async Task<AccountResponse> AddAsync(ManageAccountRequest request)
    {
        await ValidateRequestAsync(request);
        if (string.IsNullOrWhiteSpace(request.Password)) throw new ValidationException(new[] { new ValidationFailure(nameof(request.Password), "Password is required.") });
        if (await _unitOfWork.Users.ExistsUsernameAsync(request.Username)) throw new InvalidOperationException("Username already exists.");
        if (await _unitOfWork.Users.ExistsEmailAsync(request.Email)) throw new InvalidOperationException("Email already exists.");
        if (!string.IsNullOrWhiteSpace(request.Phone) && await _unitOfWork.Users.ExistsPhoneAsync(request.Phone)) throw new InvalidOperationException("Phone number already exists.");
        var role = await _unitOfWork.Roles.GetByNameAsync(request.RoleName);
        if (role is null) throw new NotFoundException("Role not found.");
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var user = new User
            {
                Username = request.Username.Trim(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim(),
                Phone = Normalize(request.Phone),
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            await EnsureUserRoleAsync(user, role);
            await SyncProfilesForRoleAsync(user, role);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            await _activityLogService.LogAsync(request.ActorUserId, null, "System Account Management", "Add", user.Username, "Success");
            return Map(user, role.RoleName, "Account added successfully.");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<AccountResponse> UpdateAsync(ManageAccountRequest request)
    {
        await ValidateRequestAsync(request);
        if (!request.UserId.HasValue) throw new ValidationException(new[] { new ValidationFailure(nameof(request.UserId), "User id is required.") });
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId.Value);
        if (user is null) throw new NotFoundException("Account not found.");
        if (!user.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase) && await _unitOfWork.Users.ExistsUsernameAsync(request.Username)) throw new InvalidOperationException("Username already exists.");
        if (!user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) && await _unitOfWork.Users.ExistsEmailAsync(request.Email)) throw new InvalidOperationException("Email already exists.");
        if (!string.IsNullOrWhiteSpace(request.Phone) && await _unitOfWork.Users.ExistsPhoneAsync(request.Phone, user.UserId)) throw new InvalidOperationException("Phone number already exists.");
        var role = await _unitOfWork.Roles.GetByNameAsync(request.RoleName);
        if (role is null) throw new NotFoundException("Role not found.");
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            user.Username = request.Username.Trim();
            user.FullName = request.FullName.Trim();
            user.Email = request.Email.Trim();
            user.Phone = Normalize(request.Phone);
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await EnsureUserRoleAsync(user, role);
            await SyncProfilesForRoleAsync(user, role);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            await _activityLogService.LogAsync(request.ActorUserId, null, "System Account Management", "Update", user.Username, "Success");
            return Map(user, role.RoleName, "Account updated successfully.");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<AccountResponse> SetLockAsync(int userId, int actorUserId, string actorRole, bool isLocked)
    {
        EnsureManager(actorRole);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null) throw new NotFoundException("Account not found.");
        if (isLocked && await IsLastManagerAsync(user)) throw new InvalidOperationException("Cannot lock the last Manager account.");
        user.IsActive = !isLocked;
        user.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        await _activityLogService.LogAsync(actorUserId, null, "System Account Management", isLocked ? "Lock" : "Unlock", user.Username, "Success");
        return Map(user, string.Empty, isLocked ? "Account locked successfully." : "Account unlocked successfully.");
    }

    public async Task<AccountResponse> DeleteAsync(int userId, int actorUserId, string actorRole)
    {
        EnsureManager(actorRole);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null) throw new NotFoundException("Account not found.");
        if (await IsLastManagerAsync(user)) throw new InvalidOperationException("Cannot delete the last Manager account.");
        if (await _unitOfWork.Users.HasOperationalReferencesAsync(user.UserId)) throw new InvalidOperationException("Account is being used in system operations.");
        _unitOfWork.Users.Delete(user);
        await _unitOfWork.SaveChangesAsync();
        await _activityLogService.LogAsync(actorUserId, null, "System Account Management", "Delete", user.Username, "Success");
        return Map(user, string.Empty, "Account deleted successfully.");
    }

    private async Task ValidateRequestAsync(ManageAccountRequest request)
    {
        ThrowIfInvalid(await _validator.ValidateAsync(request));
        EnsureManager(request.ActorRole);
    }

    private async Task<bool> IsLastManagerAsync(User user)
    {
        var roles = await _unitOfWork.UserRoles.GetByUserIdAsync(user.UserId);
        return roles.Any(role => role.Role?.RoleName == RoleConstants.Manager) && await _unitOfWork.Users.CountManagersAsync() <= 1;
    }

    private async Task EnsureUserRoleAsync(User user, Role role)
    {
        var userRoles = await _unitOfWork.UserRoles.GetByUserIdAsync(user.UserId) ?? Array.Empty<UserRole>();
        var existingRole = userRoles.FirstOrDefault();
        if (existingRole is null)
        {
            await _unitOfWork.UserRoles.AddAsync(new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                User = user,
                Role = role
            });
            return;
        }

        existingRole.RoleId = role.RoleId;
        existingRole.Role = role;
        _unitOfWork.UserRoles.Update(existingRole);
    }

    private async Task SyncProfilesForRoleAsync(User user, Role role)
    {
        if (IsCustomerRole(role.RoleName))
        {
            await EnsureCustomerProfileAsync(user);
            await RemoveEmployeeProfileAsync(user);
            return;
        }

        await RemoveCustomerProfileAsync(user);
        await EnsureEmployeeProfileAsync(user, role);
    }

    private async Task EnsureCustomerProfileAsync(User user)
    {
        var existingCustomerForUser = await _unitOfWork.Customers.GetByUserIdAsync(user.UserId);
        if (existingCustomerForUser is not null)
        {
            var updatedCustomerCode = user.Username.Trim();
            var customerWithCode = await _unitOfWork.Customers.GetByCodeAsync(updatedCustomerCode);
            if (customerWithCode is not null && customerWithCode.CustomerId != existingCustomerForUser.CustomerId)
            {
                throw new InvalidOperationException("Customer code already belongs to another user.");
            }

            existingCustomerForUser.CustomerCode = updatedCustomerCode;
            existingCustomerForUser.FullName = user.FullName.Trim();
            existingCustomerForUser.Phone = Normalize(user.Phone);
            existingCustomerForUser.Email = user.Email.Trim();
            existingCustomerForUser.IsActive = user.IsActive;
            existingCustomerForUser.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Customers.Update(existingCustomerForUser);
            return;
        }

        var customerCode = user.Username.Trim();
        var existingCustomer = await _unitOfWork.Customers.GetByCodeAsync(customerCode);
        if (existingCustomer is not null)
        {
            if (existingCustomer.UserId.HasValue && existingCustomer.UserId.Value != user.UserId)
            {
                throw new InvalidOperationException("Customer code already belongs to another user.");
            }

            existingCustomer.UserId = user.UserId;
            existingCustomer.User = user;
            existingCustomer.FullName = user.FullName.Trim();
            existingCustomer.Phone = Normalize(user.Phone);
            existingCustomer.Email = user.Email.Trim();
            existingCustomer.IsActive = user.IsActive;
            existingCustomer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Customers.Update(existingCustomer);
            return;
        }

        await _unitOfWork.Customers.AddAsync(new Customer
        {
            UserId = user.UserId,
            User = user,
            CustomerCode = customerCode,
            FullName = user.FullName.Trim(),
            Phone = Normalize(user.Phone),
            Email = user.Email.Trim(),
            IsActive = user.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    private async Task EnsureEmployeeProfileAsync(User user, Role role)
    {
        var existingEmployee = await _unitOfWork.Employees.GetByUserIdAsync(user.UserId);
        if (existingEmployee is not null)
        {
            var updatedEmployeeCode = user.Username.Trim();
            var existingEmployeeWithCode = await _unitOfWork.Employees.GetByCodeAsync(updatedEmployeeCode);
            if (existingEmployeeWithCode is not null && existingEmployeeWithCode.EmployeeId != existingEmployee.EmployeeId)
            {
                throw new InvalidOperationException("Employee code already belongs to another user.");
            }

            existingEmployee.RoleId = role.RoleId;
            existingEmployee.EmployeeCode = updatedEmployeeCode;
            existingEmployee.FullName = user.FullName.Trim();
            existingEmployee.Email = user.Email.Trim();
            existingEmployee.Phone = Normalize(user.Phone);
            existingEmployee.WorkStatus = string.IsNullOrWhiteSpace(existingEmployee.WorkStatus) ? "Working" : existingEmployee.WorkStatus;
            existingEmployee.IsActive = user.IsActive;
            existingEmployee.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Employees.Update(existingEmployee);
            return;
        }

        var employeeCode = user.Username.Trim();
        var employeeWithCode = await _unitOfWork.Employees.GetByCodeAsync(employeeCode);
        if (employeeWithCode is not null && employeeWithCode.UserId != user.UserId)
        {
            throw new InvalidOperationException("Employee code already belongs to another user.");
        }

        await _unitOfWork.Employees.AddAsync(new Employee
        {
            UserId = user.UserId,
            User = user,
            RoleId = role.RoleId,
            Role = role,
            EmployeeCode = employeeCode,
            FullName = user.FullName.Trim(),
            Email = user.Email.Trim(),
            Phone = Normalize(user.Phone),
            WorkStatus = "Working",
            IsActive = user.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    private async Task RemoveCustomerProfileAsync(User user)
    {
        var existingCustomer = await _unitOfWork.Customers.GetByUserIdAsync(user.UserId);
        if (existingCustomer is null)
        {
            return;
        }

        if (await _unitOfWork.Customers.HasOperationalReferencesAsync(existingCustomer.CustomerId))
        {
            throw new InvalidOperationException("Cannot remove customer profile because it has operational records.");
        }

        _unitOfWork.Customers.Delete(existingCustomer);
    }

    private async Task RemoveEmployeeProfileAsync(User user)
    {
        var existingEmployee = await _unitOfWork.Employees.GetByUserIdAsync(user.UserId);
        if (existingEmployee is null)
        {
            return;
        }

        _unitOfWork.Employees.Delete(existingEmployee);
    }

    private static bool IsCustomerRole(string roleName)
    {
        return roleName.Equals(RoleConstants.Customer, StringComparison.OrdinalIgnoreCase);
    }

    private static AccountResponse Map(User user, string roleName, string message) => new()
    {
        UserId = user.UserId,
        Username = user.Username,
        FullName = user.FullName,
        Email = user.Email,
        Phone = user.Phone,
        RoleName = roleName,
        IsActive = user.IsActive,
        Message = message
    };

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void EnsureManager(string role)
    {
        if (!role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase) &&
            !role.Equals(RoleConstants.Admin, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Managers or Admins can manage accounts.");
        }
    }
    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid) throw new ValidationException(result.Errors);
    }
}
