using Microsoft.EntityFrameworkCore;
using R_SS.BLL.Constants;
using R_SS.BLL.Interfaces;
using R_SS.DAL.Data;
using R_SS.Models.Entities;

namespace R_SS.Web.Services;

public static class DefaultAdministratorSeeder
{
    private static readonly (string Name, string Description)[] DefaultRoles =
    [
        (RoleConstants.Admin, "Full system administration."),
        (RoleConstants.Manager, "Operations management."),
        (RoleConstants.Receptionist, "Front desk and device reception."),
        (RoleConstants.Technician, "Technical repair execution."),
        (RoleConstants.Customer, "Customer self-service access."),
        ("Warehouse", "Inventory and supplier management."),
        ("ProductManagement", "Product catalog and order management."),
        ("Payment", "Payment and invoice processing."),
        ("Reports", "Business reporting and analytics."),
        ("SystemAdmin", "Audit log and system configuration access.")
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue("DefaultAdministrator:Enabled", true))
        {
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DefaultAdministratorSeeder");

        foreach (var (name, description) in DefaultRoles)
        {
            if (!await dbContext.Roles.AnyAsync(role => role.RoleName.ToLower() == name.ToLower()))
            {
                await dbContext.Roles.AddAsync(new Role
                {
                    RoleName = name,
                    Description = description
                });
            }
        }

        await dbContext.SaveChangesAsync();

        var adminRole = await dbContext.Roles.FirstAsync(role => role.RoleName == RoleConstants.Admin);
        var hasAdminUser = await dbContext.UserRoles
            .AnyAsync(userRole => userRole.RoleId == adminRole.RoleId && userRole.User.IsActive);
        if (hasAdminUser)
        {
            return;
        }

        var username = configuration["DefaultAdministrator:Username"] ?? "admin";
        var email = configuration["DefaultAdministrator:Email"] ?? "admin@rss.local";
        var fullName = configuration["DefaultAdministrator:FullName"] ?? "System Administrator";
        var phone = configuration["DefaultAdministrator:Phone"];
        var password = configuration["DefaultAdministrator:Password"] ?? "Admin@123456";

        var user = await dbContext.Users
            .Include(existingUser => existingUser.UserRoles)
            .FirstOrDefaultAsync(existingUser => existingUser.Username == username || existingUser.Email == email);

        if (user is null)
        {
            user = new User
            {
                Username = username.Trim(),
                Email = email.Trim(),
                FullName = fullName.Trim(),
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
                PasswordHash = passwordHasher.Hash(password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.UserRoles.AnyAsync(userRole => userRole.UserId == user.UserId && userRole.RoleId == adminRole.RoleId))
        {
            await dbContext.UserRoles.AddAsync(new UserRole
            {
                UserId = user.UserId,
                RoleId = adminRole.RoleId,
                User = user,
                Role = adminRole
            });
        }

        if (!await dbContext.Employees.AnyAsync(employee => employee.UserId == user.UserId))
        {
            await dbContext.Employees.AddAsync(new Employee
            {
                UserId = user.UserId,
                RoleId = adminRole.RoleId,
                EmployeeCode = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                WorkStatus = "Working",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync();
        logger.LogWarning("Default administrator account is available. Username: {Username}. Change this password after first login.", username);
    }
}
