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
        mocks.Employees.Setup(repo => repo.GetByUserIdAsync(It.IsAny<int>())).ReturnsAsync((Employee?)null);
        mocks.Employees.Setup(repo => repo.AddAsync(It.IsAny<Employee>())).Returns(Task.CompletedTask);
        mocks.Hasher.Setup(hasher => hasher.Hash("Password123!")).Returns("hash");
        var service = CreateService(mocks);

        var response = await service.AddAsync(BuildRequest());

        response.Message.Should().Be("Account added successfully.");
        mocks.Users.Verify(repo => repo.AddAsync(It.Is<User>(user => user.PasswordHash == "hash")), Times.Once);
        mocks.UserRoles.Verify(repo => repo.AddAsync(It.IsAny<UserRole>()), Times.Once);
        mocks.Employees.Verify(repo => repo.AddAsync(It.Is<Employee>(employee =>
            employee.EmployeeCode == "tech01" &&
            employee.RoleId == 3 &&
            employee.FullName == "Tech One")), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldCreateCustomerProfile_WhenRoleIsCustomer()
    {
        var mocks = CreateMocks();
        var request = BuildRequest();
        request.RoleName = RoleConstants.Customer;
        mocks.Roles.Setup(repo => repo.GetByNameAsync(RoleConstants.Customer)).ReturnsAsync(new Role { RoleId = 4, RoleName = RoleConstants.Customer });
        mocks.Customers.Setup(repo => repo.GetByCodeAsync(request.Username)).ReturnsAsync((Customer?)null);
        mocks.Customers.Setup(repo => repo.AddAsync(It.IsAny<Customer>())).Returns(Task.CompletedTask);
        mocks.Hasher.Setup(hasher => hasher.Hash("Password123!")).Returns("hash");
        var service = CreateService(mocks);

        var response = await service.AddAsync(request);

        response.RoleName.Should().Be(RoleConstants.Customer);
        mocks.Customers.Verify(repo => repo.AddAsync(It.Is<Customer>(customer =>
            customer.CustomerCode == request.Username &&
            customer.FullName == request.FullName &&
            customer.Email == request.Email &&
            customer.Phone == request.Phone)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRemoveCustomerProfileAndCreateEmployeeProfile_WhenRoleChangesToEmployee()
    {
        var mocks = CreateMocks();
        var request = BuildRequest();
        request.UserId = 1;
        request.RoleName = RoleConstants.Manager;
        var user = BuildUser(RoleConstants.Customer);
        var role = new Role { RoleId = 2, RoleName = RoleConstants.Manager };
        var customer = new Customer { CustomerId = 7, UserId = user.UserId, CustomerCode = user.Username, FullName = user.FullName };

        mocks.Users.Setup(repo => repo.GetByIdAsync(user.UserId)).ReturnsAsync(user);
        mocks.Roles.Setup(repo => repo.GetByNameAsync(RoleConstants.Manager)).ReturnsAsync(role);
        mocks.UserRoles.Setup(repo => repo.GetByUserIdAsync(user.UserId)).ReturnsAsync(new[] { new UserRole { UserId = user.UserId, User = user, Role = new Role { RoleName = RoleConstants.Customer } } });
        mocks.Customers.Setup(repo => repo.GetByUserIdAsync(user.UserId)).ReturnsAsync(customer);
        mocks.Employees.Setup(repo => repo.GetByUserIdAsync(user.UserId)).ReturnsAsync((Employee?)null);
        mocks.Employees.Setup(repo => repo.GetByCodeAsync(request.Username)).ReturnsAsync((Employee?)null);
        mocks.Employees.Setup(repo => repo.AddAsync(It.IsAny<Employee>())).Returns(Task.CompletedTask);
        var service = CreateService(mocks);

        var response = await service.UpdateAsync(request);

        response.RoleName.Should().Be(RoleConstants.Manager);
        mocks.Customers.Verify(repo => repo.Delete(customer), Times.Once);
        mocks.Employees.Verify(repo => repo.AddAsync(It.Is<Employee>(employee =>
            employee.UserId == user.UserId &&
            employee.RoleId == role.RoleId &&
            employee.EmployeeCode == request.Username)), Times.Once);
        mocks.UnitOfWork.Verify(unit => unit.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        mocks.UnitOfWork.Verify(unit => unit.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRemoveEmployeeProfileAndCreateCustomerProfile_WhenRoleChangesToCustomer()
    {
        var mocks = CreateMocks();
        var request = BuildRequest();
        request.UserId = 1;
        request.RoleName = RoleConstants.Customer;
        var user = BuildUser(RoleConstants.Technician);
        var role = new Role { RoleId = 4, RoleName = RoleConstants.Customer };
        var employee = new Employee { EmployeeId = 8, UserId = user.UserId, EmployeeCode = user.Username, FullName = user.FullName };

        mocks.Users.Setup(repo => repo.GetByIdAsync(user.UserId)).ReturnsAsync(user);
        mocks.Roles.Setup(repo => repo.GetByNameAsync(RoleConstants.Customer)).ReturnsAsync(role);
        mocks.UserRoles.Setup(repo => repo.GetByUserIdAsync(user.UserId)).ReturnsAsync(new[] { new UserRole { UserId = user.UserId, User = user, Role = new Role { RoleName = RoleConstants.Technician } } });
        mocks.Customers.Setup(repo => repo.GetByUserIdAsync(user.UserId)).ReturnsAsync((Customer?)null);
        mocks.Customers.Setup(repo => repo.GetByCodeAsync(request.Username)).ReturnsAsync((Customer?)null);
        mocks.Customers.Setup(repo => repo.AddAsync(It.IsAny<Customer>())).Returns(Task.CompletedTask);
        mocks.Employees.Setup(repo => repo.GetByUserIdAsync(user.UserId)).ReturnsAsync(employee);
        var service = CreateService(mocks);

        var response = await service.UpdateAsync(request);

        response.RoleName.Should().Be(RoleConstants.Customer);
        mocks.Employees.Verify(repo => repo.Delete(employee), Times.Once);
        mocks.Customers.Verify(repo => repo.AddAsync(It.Is<Customer>(customer =>
            customer.UserId == user.UserId &&
            customer.CustomerCode == request.Username)), Times.Once);
        mocks.UnitOfWork.Verify(unit => unit.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        mocks.UnitOfWork.Verify(unit => unit.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
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
        mocks.UnitOfWork.SetupGet(unit => unit.Customers).Returns(mocks.Customers.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.Employees).Returns(mocks.Employees.Object);
        mocks.Customers.Setup(repo => repo.HasOperationalReferencesAsync(It.IsAny<int>())).ReturnsAsync(false);
        mocks.UnitOfWork.Setup(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Mock.Of<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>());
        mocks.UnitOfWork.Setup(unit => unit.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mocks.UnitOfWork.Setup(unit => unit.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mocks.UnitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mocks.Logs.Setup(log => log.LogAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(Task.CompletedTask);
        return mocks;
    }

    private static ManageAccountRequest BuildRequest() => new() { ActorUserId = 2, ActorRole = RoleConstants.Manager, Username = "tech01", Password = "Password123!", FullName = "Tech One", Email = "tech@example.com", RoleName = RoleConstants.Technician };
    private static User BuildUser(string role) => new() { 
        UserId = 1, 
        Username = role, 
        PasswordHash = "hash", 
        Email = "manager@example.com", 
        FullName = "Manager", 
        IsActive = true };

    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IUserRp> Users { get; } = new();
        public Mock<IRoleRp> Roles { get; } = new();
        public Mock<IUserRoleRp> UserRoles { get; } = new();
        public Mock<ICustomerRp> Customers { get; } = new();
        public Mock<IEmployeeRp> Employees { get; } = new();
        public Mock<IPasswordHasher> Hasher { get; } = new();
        public Mock<ISystemActivityLogService> Logs { get; } = new();
    }
}
