using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Authentication;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Helpers;
using R_SS.BLL.Mapping;
using R_SS.BLL.Services;
using R_SS.BLL.Interfaces;
using R_SS.BLL.Validators.Authentication;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.AuthTests;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndAssignCustomerRole_WhenDataIsValid()
    {
        var userRepoMock = new Mock<IUserRp>();
        var roleRepoMock = new Mock<IRoleRp>();
        var userRoleRepoMock = new Mock<IUserRoleRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(userRepoMock, roleRepoMock, userRoleRepoMock);
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenGeneratorMock = new Mock<IJwtTokenGenerator>();

        var request = BuildRegisterRequest();
        var customerRole = new Role { RoleId = 2, RoleName = RoleConstants.Customer };
        var savedUser = default(User);
        var savedUserRole = default(UserRole);

        userRepoMock.Setup(x => x.ExistsUsernameAsync(request.Username)).ReturnsAsync(false);
        userRepoMock.Setup(x => x.ExistsEmailAsync(request.Email)).ReturnsAsync(false);
        userRepoMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(user =>
            {
                user.UserId = 123;
                savedUser = user;
            })
            .Returns(Task.CompletedTask);

        roleRepoMock.Setup(x => x.GetByNameAsync(RoleConstants.Customer)).ReturnsAsync(customerRole);
        userRoleRepoMock
            .Setup(x => x.AddAsync(It.IsAny<UserRole>()))
            .Callback<UserRole>(userRole =>
            {
                savedUserRole = userRole;
            })
            .Returns(Task.CompletedTask);

        var service = CreateService(unitOfWorkMock.Object, passwordHasherMock.Object, tokenGeneratorMock.Object);

        var response = await service.RegisterAsync(request);

        response.UserId.Should().Be(123);
        response.Username.Should().Be(request.Username);
        response.Email.Should().Be(request.Email);
        response.FullName.Should().Be(request.FullName);
        response.Message.Should().Be("Register successfully.");

        passwordHasherMock.Verify(x => x.Hash(request.Password), Times.Once);
        userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        roleRepoMock.Verify(x => x.GetByNameAsync(RoleConstants.Customer), Times.Once);
        userRoleRepoMock.Verify(x => x.AddAsync(It.IsAny<UserRole>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

        savedUser.Should().NotBeNull();
        savedUserRole.Should().NotBeNull();
        savedUserRole!.User.Should().BeSameAs(savedUser);
        savedUserRole.Role.Should().BeSameAs(customerRole);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenUsernameAlreadyExists()
    {
        var userRepoMock = new Mock<IUserRp>();
        var roleRepoMock = new Mock<IRoleRp>();
        var userRoleRepoMock = new Mock<IUserRoleRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(userRepoMock, roleRepoMock, userRoleRepoMock);
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var request = BuildRegisterRequest();

        userRepoMock.Setup(x => x.ExistsUsernameAsync(request.Username)).ReturnsAsync(true);
        userRepoMock.Setup(x => x.ExistsEmailAsync(request.Email)).ReturnsAsync(false);

        var service = CreateService(unitOfWorkMock.Object, passwordHasherMock.Object, tokenGeneratorMock.Object);

        var act = async () => await service.RegisterAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Username already exists.");

        userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var userRepoMock = new Mock<IUserRp>();
        var roleRepoMock = new Mock<IRoleRp>();
        var userRoleRepoMock = new Mock<IUserRoleRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(userRepoMock, roleRepoMock, userRoleRepoMock);
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var request = BuildRegisterRequest();

        userRepoMock.Setup(x => x.ExistsUsernameAsync(request.Username)).ReturnsAsync(false);
        userRepoMock.Setup(x => x.ExistsEmailAsync(request.Email)).ReturnsAsync(true);

        var service = CreateService(unitOfWorkMock.Object, passwordHasherMock.Object, tokenGeneratorMock.Object);

        var act = async () => await service.RegisterAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already exists.");

        userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowValidationException_WhenPasswordDoesNotMatchConfirmPassword()
    {
        var service = CreateService(
            new Mock<IUnitOfWork>().Object,
            new Mock<IPasswordHasher>().Object,
            new Mock<IJwtTokenGenerator>().Object);

        var act = async () => await service.RegisterAsync(new RegisterRequest
        {
            Username = "john.doe",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!",
            Email = "john.doe@example.com",
            FullName = "John Doe"
        });

        var exception = await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        exception.Which.Errors.Should().ContainSingle(error =>
            error.PropertyName == nameof(RegisterRequest.ConfirmPassword));
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenCustomerRoleIsMissing()
    {
        var userRepoMock = new Mock<IUserRp>();
        var roleRepoMock = new Mock<IRoleRp>();
        var userRoleRepoMock = new Mock<IUserRoleRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(userRepoMock, roleRepoMock, userRoleRepoMock);
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var request = BuildRegisterRequest();

        userRepoMock.Setup(x => x.ExistsUsernameAsync(request.Username)).ReturnsAsync(false);
        userRepoMock.Setup(x => x.ExistsEmailAsync(request.Email)).ReturnsAsync(false);
        userRepoMock.Setup(x => x.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        roleRepoMock.Setup(x => x.GetByNameAsync(RoleConstants.Customer)).ReturnsAsync((Role?)null);

        var service = CreateService(unitOfWorkMock.Object, passwordHasherMock.Object, tokenGeneratorMock.Object);

        var act = async () => await service.RegisterAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Customer role not found.");

        unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        userRoleRepoMock.Verify(x => x.AddAsync(It.IsAny<UserRole>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldRollback_WhenAddUserThrows()
    {
        var userRepoMock = new Mock<IUserRp>();
        var roleRepoMock = new Mock<IRoleRp>();
        var userRoleRepoMock = new Mock<IUserRoleRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(userRepoMock, roleRepoMock, userRoleRepoMock);
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var request = BuildRegisterRequest();
        var customerRole = new Role { RoleId = 2, RoleName = RoleConstants.Customer };

        userRepoMock.Setup(x => x.ExistsUsernameAsync(request.Username)).ReturnsAsync(false);
        userRepoMock.Setup(x => x.ExistsEmailAsync(request.Email)).ReturnsAsync(false);
        userRepoMock.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
        roleRepoMock.Setup(x => x.GetByNameAsync(RoleConstants.Customer)).ReturnsAsync(customerRole);

        var service = CreateService(unitOfWorkMock.Object, passwordHasherMock.Object, tokenGeneratorMock.Object);

        var act = async () => await service.RegisterAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("DB error");

        unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        userRoleRepoMock.Verify(x => x.AddAsync(It.IsAny<UserRole>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokenAndRole_WhenCredentialsAreValid()
    {
        var user = BuildUser();
        var service = CreateService(user, passwordMatches: true, roleName: "Customer");

        var response = await service.LoginAsync(new LoginRequest
        {
            EmailOrUsername = "john.doe@example.com",
            Password = "Password123!"
        });

        response.UserId.Should().Be(user.UserId);
        response.Username.Should().Be(user.Username);
        response.Email.Should().Be(user.Email);
        response.RoleName.Should().Be("Customer");
        response.AccessToken.Should().Be("token-value");
        response.Message.Should().Be("Login successfully.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenPasswordIsWrong()
    {
        var user = BuildUser();
        var service = CreateService(user, passwordMatches: false, roleName: "Customer");

        var act = async () => await service.LoginAsync(new LoginRequest
        {
            EmailOrUsername = "john.doe@example.com",
            Password = "WrongPassword123!"
        });

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Incorrect password.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var service = CreateService(null, passwordMatches: false, roleName: "Customer");

        var act = async () => await service.LoginAsync(new LoginRequest
        {
            EmailOrUsername = "missing@example.com",
            Password = "Password123!"
        });

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Account not found.");
    }

    private AuthService CreateService(User? user, bool passwordMatches, string roleName)
    {
        var userRepoMock = new Mock<IUserRp>();
        var roleRepoMock = new Mock<IRoleRp>();
        var userRoleRepoMock = new Mock<IUserRoleRp>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenGeneratorMock = new Mock<IJwtTokenGenerator>();

        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepoMock.Object);
        unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepoMock.Object);
        unitOfWorkMock.SetupGet(x => x.UserRoles).Returns(userRoleRepoMock.Object);

        userRepoMock
            .Setup(x => x.GetByIdentifierAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(passwordMatches);

        tokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<User>(), It.IsAny<string>()))
            .Returns(new JwtTokenResult
            {
                AccessToken = "token-value",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60)
            });

        userRoleRepoMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new[]
            {
                new UserRole
                {
                    UserRoleId = 1,
                    UserId = user?.UserId ?? 0,
                    RoleId = 1,
                    User = user ?? BuildUser(),
                    Role = new Role { RoleId = 1, RoleName = roleName }
                }
            });

        return new AuthService(
            unitOfWorkMock.Object,
            passwordHasherMock.Object,
            tokenGeneratorMock.Object,
            new RegisterRequestValidator(),
            new LoginRequestValidator());
    }

    private static AuthService CreateService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        return new AuthService(
            unitOfWork,
            passwordHasher,
            jwtTokenGenerator,
            new RegisterRequestValidator(),
            new LoginRequestValidator());
    }

    private static Mock<IUnitOfWork> CreateUnitOfWorkMock(
        Mock<IUserRp> userRepoMock,
        Mock<IRoleRp> roleRepoMock,
        Mock<IUserRoleRp> userRoleRepoMock)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepoMock.Object);
        unitOfWorkMock.SetupGet(x => x.Roles).Returns(roleRepoMock.Object);
        unitOfWorkMock.SetupGet(x => x.UserRoles).Returns(userRoleRepoMock.Object);
        unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IDbContextTransaction>());
        unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        return unitOfWorkMock;
    }

    private static RegisterRequest BuildRegisterRequest() => new()
    {
        Username = "john.doe",
        Password = "Password123!",
        ConfirmPassword = "Password123!",
        Email = "john.doe@example.com",
        FullName = "John Doe",
        Phone = "0123456789",
        Address = "123 Main Street"
    };

    private static User BuildUser()
    {
        return new User
        {
            UserId = 1,
            Username = "john.doe",
            PasswordHash = "hashed-password",
            Email = "john.doe@example.com",
            FullName = "John Doe",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
