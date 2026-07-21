using FluentAssertions;
using R_SS.BLL.Constants;
using R_SS.DAL.Repositories;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;
using R_SS.Tests.Helpers;

namespace R_SS.Tests.RepositoryTests;

public class AccountProfileClassificationTests
{
    [Fact]
    public async Task CustomerQueries_ShouldExcludeProfilesWhoseCurrentUserRoleIsEmployee()
    {
        await using var context = TestDbContextFactory.Create();
        var customerRole = new Role { RoleName = RoleConstants.Customer };
        var technicianRole = new Role { RoleName = RoleConstants.Technician };
        var customerUser = BuildUser("customer01", "Customer One");
        var employeeUser = BuildUser("tech01", "Tech One");

        context.Roles.AddRange(customerRole, technicianRole);
        context.Users.AddRange(customerUser, employeeUser);
        context.UserRoles.AddRange(
            new UserRole { User = customerUser, Role = customerRole },
            new UserRole { User = employeeUser, Role = technicianRole });
        context.Customers.AddRange(
            BuildCustomer("C001", "Customer One", customerUser),
            BuildCustomer("C002", "Tech One", employeeUser),
            BuildCustomer("C003", "Walk In", null));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        ICustomerRp repository = new CustomerRp(context);

        var allCustomers = (await repository.GetAllAsync()).ToArray();
        var searchResults = await repository.SearchAsync("One");

        allCustomers.Select(customer => customer.CustomerCode).Should().BeEquivalentTo("C001", "C003");
        searchResults.Select(customer => customer.CustomerCode).Should().BeEquivalentTo("C001");
    }

    [Fact]
    public async Task EmployeeQueries_ShouldExcludeProfilesWhoseCurrentUserRoleIsCustomer()
    {
        await using var context = TestDbContextFactory.Create();
        var customerRole = new Role { RoleName = RoleConstants.Customer };
        var technicianRole = new Role { RoleName = RoleConstants.Technician };
        var customerUser = BuildUser("customer01", "Customer One");
        var employeeUser = BuildUser("tech01", "Tech One");

        context.Roles.AddRange(customerRole, technicianRole);
        context.Users.AddRange(customerUser, employeeUser);
        context.UserRoles.AddRange(
            new UserRole { User = customerUser, Role = customerRole },
            new UserRole { User = employeeUser, Role = technicianRole });
        context.Employees.AddRange(
            BuildEmployee("E001", "Customer One", customerUser, customerRole),
            BuildEmployee("E002", "Tech One", employeeUser, technicianRole));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        IEmployeeRp repository = new EmployeeRp(context);

        var employees = (await repository.GetAllAsync()).ToArray();

        employees.Should().ContainSingle();
        employees[0].EmployeeCode.Should().Be("E002");
    }

    private static User BuildUser(string username, string fullName) => new()
    {
        Username = username,
        PasswordHash = "hash",
        Email = $"{username}@example.com",
        FullName = fullName,
        IsActive = true
    };

    private static Customer BuildCustomer(string code, string fullName, User? user) => new()
    {
        User = user,
        CustomerCode = code,
        FullName = fullName,
        Email = $"{code.ToLowerInvariant()}@example.com",
        IsActive = true
    };

    private static Employee BuildEmployee(string code, string fullName, User user, Role role) => new()
    {
        User = user,
        Role = role,
        EmployeeCode = code,
        FullName = fullName,
        Email = $"{code.ToLowerInvariant()}@example.com",
        WorkStatus = "Working",
        IsActive = true
    };
}
