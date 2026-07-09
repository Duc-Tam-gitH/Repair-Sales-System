using FluentValidation;
using FluentValidation.Results;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    /// <summary>
    /// Registers a new user account and assigns the default customer role.
    /// </summary>
    /// <param name="request">Registration request data.</param>
    /// <returns>The registration response.</returns>
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        await ValidateRegisterRequestAsync(request);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var user = await CreateUserAsync(request);
            await AssignDefaultRoleAsync(user);

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
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        await ValidateLoginRequestAsync(request);

        var user = await _unitOfWork.Users.GetByIdentifierAsync(request.EmailOrUsername);
        if (user is null)
        {
            throw new NotFoundException("Account not found.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is not active.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Incorrect password.");
        }

        var roleName = await GetPrimaryRoleNameAsync(user.UserId);
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new NotFoundException("User role not found.");
        }

        var tokenResult = _jwtTokenGenerator.GenerateToken(user, roleName);
        return CreateLoginResponse(user, roleName, tokenResult);
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

    private RegisterResponse CreateRegisterResponse(User user)
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

    private LoginResponse CreateLoginResponse(User user, string roleName, JwtTokenResult tokenResult)
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

    private async Task AssignDefaultRoleAsync(User user)
    {
        var customerRole = await _unitOfWork.Roles.GetByNameAsync(RoleConstants.Customer);
        if (customerRole is null)
        {
            throw new InvalidOperationException("Customer role not found.");
        }

        await _unitOfWork.UserRoles.AddAsync(new UserRole
        {
            User = user,
            Role = customerRole
        });
    }

    private async Task<string?> GetPrimaryRoleNameAsync(int userId)
    {
        var userRoles = await _unitOfWork.UserRoles.GetByUserIdAsync(userId);
        var userRole = userRoles.FirstOrDefault();
        return userRole?.Role?.RoleName;
    }
}
