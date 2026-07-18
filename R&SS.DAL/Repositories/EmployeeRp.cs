using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class EmployeeRp : GenericRp<Employee>, IEmployeeRp
    {
        public EmployeeRp(AppDbContext context) : base(context)
        {
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
