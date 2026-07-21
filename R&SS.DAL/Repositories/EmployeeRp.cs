using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class EmployeeRp : GenericRp<Employee>, IEmployeeRp
    {
        private static readonly string[] EmployeeRoleNames =
        {
            "Admin",
            "Manager",
            "Receptionist",
            "Technician"
        };

        public EmployeeRp(AppDbContext context) : base(context)
        {
        }

        public new async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .AsNoTracking()
                .Include(employee => employee.Role)
                .Include(employee => employee.User)
                .ThenInclude(user => user!.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .Where(employee =>
                    employee.User != null &&
                    employee.User.UserRoles.Any(userRole =>
                        userRole.Role != null &&
                        EmployeeRoleNames.Contains(userRole.Role.RoleName)))
                .ToListAsync();
        }

        public async Task<Employee?> GetByUserIdAsync(int userId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(employee => employee.UserId == userId);
        }

        public async Task<Employee?> GetByCodeAsync(string employeeCode)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(employee => employee.EmployeeCode.ToLower() == employeeCode.ToLower());
        }
    }
}
