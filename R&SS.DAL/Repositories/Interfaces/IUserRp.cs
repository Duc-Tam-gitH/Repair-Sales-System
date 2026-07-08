using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IUserRp : IGenericRp<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsUsernameAsync(string username);
        Task<bool> ExistsEmailAsync(string email);
    }
}
