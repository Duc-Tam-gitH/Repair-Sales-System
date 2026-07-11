using FluentAssertions;
using FluentValidation;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Account;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Account;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.AccountTests;

public class AccountManagementServiceTests
{
    [Fact]
    public async Task AddAsync_ShouldCreateUserWithHashedPasswordAndRole_WhenRequestIsValid()
    {
        var mocks = CreateMocks();
        mocks.Roles.Setup(repo => repo.GetByNameAsync(RoleConstants.Technician)).ReturnsAsync(new Role { RoleId = 3, RoleName = RoleConstants.Technician });
        mocks.Hasher.Setup(hasher => hasher.Hash("Password123!")).Returns("hash");
        var service = CreateService(mocks);

        var response = await service.AddAsync(BuildRequest());

        response.Message.Should().Be("Account added successfully.");
        mocks.Users.Verify(repo => repo.AddAsync(It.Is<User>(user => user.PasswordHash == "hash")), Times.Once);
        mocks.UserRoles.Verify(repo => repo.AddAsync(It.IsAny<UserRole>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowInvalidOperationException_WhenUsernameExists()
    {
        var mocks = CreateMocks();
        mocks.Users.Setup(repo => repo.ExistsUsernameAsync("tech01")).ReturnsAsync(true);
        var service = CreateService(mocks);

        var act = async () => await service.AddAsync(BuildRequest());

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Username already exists.");
    }

    [Fact]
    public async Task SetLockAsync_ShouldThrowInvalidOperationException_WhenLockingLastManager()
    {
        var mocks = CreateMocks();
        var manager = BuildUser(RoleConstants.Manager);
        mocks.Users.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(manager);
        mocks.Users.Setup(repo => repo.CountManagersAsync()).ReturnsAsync(1);
        mocks.UserRoles.Setup(repo => repo.GetByUserIdAsync(1)).ReturnsAsync(new[] { new UserRole { UserId = 1, User = manager, Role = new Role { RoleName = RoleConstants.Manager } } });
        var service = CreateService(mocks);

        var act = async () => await service.SetLockAsync(1, 2, RoleConstants.Manager, true);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot lock the last Manager account.");
    }

    private static AccountManagementService CreateService(TestMocks mocks) => new(mocks.UnitOfWork.Object, mocks.Hasher.Object, mocks.Logs.Object, new ManageAccountRequestValidator());

    private static TestMocks CreateMocks()
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.Users).Returns(mocks.Users.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.Roles).Returns(mocks.Roles.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.UserRoles).Returns(mocks.UserRoles.Object);
        mocks.UnitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mocks.Logs.Setup(log => log.LogAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(Task.CompletedTask);
        return mocks;
    }

    private static ManageAccountRequest BuildRequest() => new() { ActorUserId = 2, ActorRole = RoleConstants.Manager, Username = "tech01", Password = "Password123!", FullName = "Tech One", Email = "tech@example.com", RoleName = RoleConstants.Technician };
    private static User BuildUser(string role) => new() { UserId = 1, Username = role, PasswordHash = "hash", Email = "manager@example.com", FullName = "Manager", IsActive = true };

    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IUserRp> Users { get; } = new();
        public Mock<IRoleRp> Roles { get; } = new();
        public Mock<IUserRoleRp> UserRoles { get; } = new();
        public Mock<IPasswordHasher> Hasher { get; } = new();
        public Mock<ISystemActivityLogService> Logs { get; } = new();
    }
}
