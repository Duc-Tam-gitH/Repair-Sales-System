using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IEmployeeRp : IGenericRp<Employee>
    {
        Task<Employee?> GetByUserIdAsync(int userId);
        Task<Employee?> GetByCodeAsync(string employeeCode);
    }
}
