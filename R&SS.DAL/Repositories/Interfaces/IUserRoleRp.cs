using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IUserRoleRp : IGenericRp<UserRole>
    {
        Task<IReadOnlyCollection<UserRole>> GetByUserIdAsync(int userId);
    }
}
