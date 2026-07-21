using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Authentication;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Helpers;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class AuthService : IAuthService
{
    private static readonly TimeSpan OtpValidityWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan OtpSendWindow = TimeSpan.FromHours(1);
    private static readonly TimeSpan OtpSendLockWindow = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan AccountLockWindow = TimeSpan.FromMinutes(15);

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IOtpGenerator _otpGenerator;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
    private readonly IValidator<VerifyForgotPasswordOtpRequest> _verifyOtpValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
    private readonly IValidator<UpdatePersonalInfoRequest> _updatePersonalInfoValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IOtpGenerator otpGenerator,
        IEmailSender emailSender,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<ForgotPasswordRequest> forgotPasswordValidator,
        IValidator<VerifyForgotPasswordOtpRequest> verifyOtpValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator,
        IValidator<UpdatePersonalInfoRequest> updatePersonalInfoValidator,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _otpGenerator = otpGenerator;
        _emailSender = emailSender;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _verifyOtpValidator = verifyOtpValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _changePasswordValidator = changePasswordValidator;
        _updatePersonalInfoValidator = updatePersonalInfoValidator;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account and assigns the default customer role.
    /// </summary>
    /// <param name="request">Registration request data.</param>
    /// <returns>The registration response.</returns>
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _registerValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);

        await ValidateRegisterRequestAsync(request);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var user = await CreateUserAsync(request);
            var customerRole = await GetOrCreateCustomerRoleAsync();
            await _unitOfWork.SaveChangesAsync();

            await AssignDefaultRoleAsync(user, customerRole);
            await EnsureCustomerProfileAsync(user);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return CreateRegisterResponse(user);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Authenticates a user with email/username and password.
    /// </summary>
    /// <param name="request">Login request data.</param>
    /// <returns>The login response with role and access token.</returns>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        await ValidateLoginRequestAsync(request);

        var user = await _unitOfWork.Users.GetUserWithRolesAsync(request.EmailOrUsername);
        if (user is null)
        {
            throw new NotFoundException("Account not found.");
        }

        EnsureAccountIsActiveAndNotLocked(user);

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is not active.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Incorrect password.");
        }

        var roleName = GetPrimaryRoleName(user);
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new NotFoundException("User role not found.");
        }

        var tokenResult = _jwtTokenGenerator.GenerateToken(user, roleName);
        return CreateLoginResponse(user, roleName, tokenResult);
    }

    /// <summary>
    /// Starts a password reset flow by generating and emailing an OTP code.
    /// </summary>
    /// <param name="request">Forgot password request data.</param>
    /// <returns>The password reset request result.</returns>
    public async Task<ForgotPasswordResponse> RequestPasswordResetAsync(ForgotPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _forgotPasswordValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);

        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (user is null)
        {
            throw new NotFoundException("Email does not exist.");
        }

        EnsureAccountIsActiveAndNotLocked(user);

        var now = DateTime.UtcNow;
        var resetRequest = await _unitOfWork.PasswordResetRequests.GetByUserIdAsync(user.UserId);
        if (resetRequest is null)
        {
            resetRequest = CreateResetRequest(user, now);
            await _unitOfWork.PasswordResetRequests.AddAsync(resetRequest);
        }
        else
        {
            EnsureResetFunctionIsNotLocked(resetRequest, now);
            NormalizeSendWindow(resetRequest, now);

            if (resetRequest.SendAttemptCount >= 3)
            {
                LockResetFunction(resetRequest, now);
                await _unitOfWork.SaveChangesAsync();
                throw new InvalidOperationException("Password reset function is locked for 30 minutes.");
            }
        }

        var otpCode = _otpGenerator.Generate(6);
        ApplyNewOtp(resetRequest, otpCode, now);

        await _unitOfWork.SaveChangesAsync();
        await _emailSender.SendPasswordResetOtpAsync(user.Email, user.FullName, otpCode);

        return new ForgotPasswordResponse
        {
            Message = "OTP sent successfully.",
            OtpExpiresAtUtc = resetRequest.OtpExpiresAtUtc
        };
    }

    /// <summary>
    /// Verifies the OTP code that was sent to the user's email address.
    /// </summary>
    /// <param name="request">OTP verification request data.</param>
    /// <returns>The verification result.</returns>
    public async Task<VerifyForgotPasswordOtpResponse> VerifyPasswordResetOtpAsync(VerifyForgotPasswordOtpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _verifyOtpValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);

        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (user is null)
        {
            throw new NotFoundException("Email does not exist.");
        }

        EnsureAccountIsActiveAndNotLocked(user);

        var resetRequest = await _unitOfWork.PasswordResetRequests.GetByUserIdAsync(user.UserId);
        if (resetRequest is null || resetRequest.IsCompleted || IsOtpExpired(resetRequest))
        {
            throw new UnauthorizedException("OTP is invalid or expired.");
        }

        if (_passwordHasher.Verify(request.OtpCode, resetRequest.OtpCodeHash))
        {
            resetRequest.IsOtpVerified = true;
            resetRequest.OtpVerifiedAtUtc = DateTime.UtcNow;
            resetRequest.OtpAttemptCount = 0;
            resetRequest.UpdatedAtUtc = DateTime.UtcNow;

            _unitOfWork.PasswordResetRequests.Update(resetRequest);
            await _unitOfWork.SaveChangesAsync();

            return new VerifyForgotPasswordOtpResponse
            {
                Message = "OTP verified successfully.",
                VerifiedAtUtc = resetRequest.OtpVerifiedAtUtc.Value
            };
        }

        resetRequest.OtpAttemptCount++;
        if (resetRequest.OtpAttemptCount > 5)
        {
            user.AccountLockedUntilUtc = DateTime.UtcNow.Add(AccountLockWindow);
            resetRequest.UpdatedAtUtc = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            _unitOfWork.PasswordResetRequests.Update(resetRequest);
            await _unitOfWork.SaveChangesAsync();
            throw new UnauthorizedException("Account is temporarily locked. Please try again later.");
        }

        resetRequest.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.PasswordResetRequests.Update(resetRequest);
        await _unitOfWork.SaveChangesAsync();

        throw new UnauthorizedException("OTP is invalid or expired.");
    }

    /// <summary>
    /// Resets the user's password after OTP verification.
    /// </summary>
    /// <param name="request">Reset password request data.</param>
    /// <returns>The password reset result.</returns>
    public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _resetPasswordValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);

        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (user is null)
        {
            throw new NotFoundException("Email does not exist.");
        }

        EnsureAccountIsActiveAndNotLocked(user);

        var resetRequest = await _unitOfWork.PasswordResetRequests.GetByUserIdAsync(user.UserId);
        if (resetRequest is null || !resetRequest.IsOtpVerified || resetRequest.IsCompleted || IsOtpExpired(resetRequest))
        {
            throw new UnauthorizedException("OTP is invalid or expired.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.AccountLockedUntilUtc = null;
        user.UpdatedAt = DateTime.UtcNow;

        resetRequest.IsCompleted = true;
        resetRequest.CompletedAtUtc = DateTime.UtcNow;
        resetRequest.UpdatedAtUtc = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        _unitOfWork.PasswordResetRequests.Update(resetRequest);
        await _unitOfWork.SaveChangesAsync();

        return new ResetPasswordResponse
        {
            Message = "Password reset successfully.",
            ResetAtUtc = resetRequest.CompletedAtUtc.Value
        };
    }

    /// <summary>
    /// Changes the password for a logged-in user.
    /// </summary>
    /// <param name="request">Change password request data.</param>
    /// <returns>The password change result.</returns>
    public async Task<ChangePasswordResponse> ChangePasswordAsync(ChangePasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _changePasswordValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user is null)
        {
            throw new NotFoundException("Account not found.");
        }

        EnsureAccountIsActiveAndNotLocked(user);

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("User {UserId} changed their password.", user.UserId);

        return new ChangePasswordResponse
        {
            Message = "Password changed successfully.",
            ChangedAtUtc = user.UpdatedAt
        };
    }

    /// <summary>
    /// Gets the personal information for a logged-in user.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <returns>The user's personal information.</returns>
    public async Task<PersonalInfoResponse> GetPersonalInfoAsync(int userId)
    {
        if (userId <= 0)
        {
            throw new UnauthorizedException("Unauthorized.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException("Account not found.");
        }

        EnsureAccountIsActiveAndNotLocked(user);

        return new PersonalInfoResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Address = user.Address,
            Message = "Personal information retrieved successfully."
        };
    }

    /// <summary>
    /// Updates the personal information for a logged-in user.
    /// </summary>
    /// <param name="request">Personal information update data.</param>
    /// <returns>The updated personal information.</returns>
    public async Task<PersonalInfoResponse> UpdatePersonalInfoAsync(UpdatePersonalInfoRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _updatePersonalInfoValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user is null)
        {
            throw new NotFoundException("Account not found.");
        }

        EnsureAccountIsActiveAndNotLocked(user);

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase) &&
            await _unitOfWork.Users.ExistsEmailAsync(request.Email))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim();
        user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        user.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("User {UserId} updated personal information.", user.UserId);

        return new PersonalInfoResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Address = user.Address,
            Message = "Personal information updated successfully."
        };
    }

    /// <summary>
    /// Ends the current authenticated session.
    /// </summary>
    /// <returns>The logout confirmation.</returns>
    public Task<LogoutResponse> LogoutAsync()
    {
        return Task.FromResult(new LogoutResponse
        {
            Message = "Logout successfully.",
            LoggedOutAtUtc = DateTime.UtcNow
        });
    }

    private async Task ValidateRegisterRequestAsync(RegisterRequest request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);

        if (await _unitOfWork.Users.ExistsUsernameAsync(request.Username))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        if (await _unitOfWork.Users.ExistsEmailAsync(request.Email))
        {
            throw new InvalidOperationException("Email already exists.");
        }
    }

    private async Task ValidateLoginRequestAsync(LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);
    }

    private static void ThrowIfInvalid(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }

    private void EnsureAccountIsActiveAndNotLocked(User user)
    {
        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is not active.");
        }

        if (user.AccountLockedUntilUtc.HasValue && user.AccountLockedUntilUtc.Value > DateTime.UtcNow)
        {
            throw new UnauthorizedException("Account is temporarily locked. Please try again later.");
        }
    }

    private static bool IsOtpExpired(PasswordResetRequest resetRequest)
    {
        return resetRequest.OtpExpiresAtUtc <= DateTime.UtcNow;
    }

    private static void NormalizeSendWindow(PasswordResetRequest resetRequest, DateTime now)
    {
        if (resetRequest.SendWindowStartedAtUtc.Add(OtpSendWindow) <= now)
        {
            resetRequest.SendWindowStartedAtUtc = now;
            resetRequest.SendAttemptCount = 0;
            resetRequest.FunctionLockedUntilUtc = null;
        }
    }

    private static void EnsureResetFunctionIsNotLocked(PasswordResetRequest resetRequest, DateTime now)
    {
        if (resetRequest.FunctionLockedUntilUtc.HasValue && resetRequest.FunctionLockedUntilUtc.Value > now)
        {
            throw new InvalidOperationException("Password reset function is locked for 30 minutes.");
        }
    }

    private static void LockResetFunction(PasswordResetRequest resetRequest, DateTime now)
    {
        resetRequest.FunctionLockedUntilUtc = now.Add(OtpSendLockWindow);
        resetRequest.UpdatedAtUtc = now;
    }

    private static PasswordResetRequest CreateResetRequest(User user, DateTime now)
    {
        return new PasswordResetRequest
        {
            User = user,
            UserId = user.UserId,
            OtpCodeHash = string.Empty,
            OtpAttemptCount = 0,
            OtpSentAtUtc = now,
            OtpExpiresAtUtc = now.Add(OtpValidityWindow),
            SendAttemptCount = 0,
            SendWindowStartedAtUtc = now,
            FunctionLockedUntilUtc = null,
            IsOtpVerified = false,
            OtpVerifiedAtUtc = null,
            IsCompleted = false,
            CompletedAtUtc = null,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    private void ApplyNewOtp(PasswordResetRequest resetRequest, string otpCode, DateTime now)
    {
        resetRequest.OtpCodeHash = _passwordHasher.Hash(otpCode);
        resetRequest.OtpSentAtUtc = now;
        resetRequest.OtpExpiresAtUtc = now.Add(OtpValidityWindow);
        resetRequest.OtpAttemptCount = 0;
        resetRequest.SendAttemptCount++;
        resetRequest.FunctionLockedUntilUtc = null;
        resetRequest.IsOtpVerified = false;
        resetRequest.OtpVerifiedAtUtc = null;
        resetRequest.IsCompleted = false;
        resetRequest.CompletedAtUtc = null;
        resetRequest.UpdatedAtUtc = now;
    }

    private async Task<User> CreateUserAsync(RegisterRequest request)
    {
        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Email = request.Email,
            FullName = request.FullName,
            Phone = request.Phone,
            Address = request.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        return user;
    }

    private static RegisterResponse CreateRegisterResponse(User user)
    {
        return new RegisterResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Message = "Register successfully."
        };
    }

    private static LoginResponse CreateLoginResponse(User user, string roleName, JwtTokenResult tokenResult)
    {
        return new LoginResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            RoleName = roleName,
            AccessToken = tokenResult.AccessToken,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc,
            Message = "Login successfully."
        };
    }

    private async Task AssignDefaultRoleAsync(User user, Role customerRole)
    {
        await _unitOfWork.UserRoles.AddAsync(new UserRole
        {
            UserId = user.UserId,
            RoleId = customerRole.RoleId,
            User = user,
            Role = customerRole
        });
    }

    private async Task EnsureCustomerProfileAsync(User user)
    {
        var existingCustomerForUser = await _unitOfWork.Customers.GetByUserIdAsync(user.UserId);
        if (existingCustomerForUser is not null)
        {
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
            existingCustomer.Phone = string.IsNullOrWhiteSpace(user.Phone) ? null : user.Phone.Trim();
            existingCustomer.Email = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.Trim();
            existingCustomer.Address = string.IsNullOrWhiteSpace(user.Address) ? null : user.Address.Trim();
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
            Phone = string.IsNullOrWhiteSpace(user.Phone) ? null : user.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.Trim(),
            Address = string.IsNullOrWhiteSpace(user.Address) ? null : user.Address.Trim(),
            IsActive = user.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    private async Task<Role> GetOrCreateCustomerRoleAsync()
    {
        var customerRole = await _unitOfWork.Roles.GetByNameAsync(RoleConstants.Customer);
        if (customerRole is not null)
        {
            return customerRole;
        }

        customerRole = new Role
        {
            RoleName = RoleConstants.Customer,
            Description = "Default customer role."
        };

        await _unitOfWork.Roles.AddAsync(customerRole);
        return customerRole;
    }

    private static string? GetPrimaryRoleName(User user)
    {
        var userRole = user.UserRoles
            .OrderBy(role => role.UserRoleId)
            .FirstOrDefault();
        return userRole?.Role?.RoleName;
    }
}
