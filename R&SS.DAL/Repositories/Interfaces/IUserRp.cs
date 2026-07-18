using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IUserRp : IGenericRp<User>
    {
        Task<IReadOnlyCollection<User>> GetAllWithRolesAsync();
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserWithRolesAsync(string emailOrUsername);
        Task<User?> GetByIdentifierAsync(string identifier);
        Task<bool> ExistsUsernameAsync(string username);
        Task<bool> ExistsEmailAsync(string email);
        Task<User?> GetActiveByEmailAsync(string email);
        Task<IReadOnlyCollection<User>> GetTechniciansAsync();
        Task<bool> ExistsPhoneAsync(string phone, int? excludedUserId = null);
        Task<int> CountManagersAsync();
        Task<bool> HasOperationalReferencesAsync(int userId);
    }
}
