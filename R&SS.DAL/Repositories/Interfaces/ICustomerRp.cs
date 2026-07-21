using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface ICustomerRp : IGenericRp<Customer>
    {
        Task<IReadOnlyCollection<Customer>> SearchAsync(string? keyword);
        Task<Customer?> GetByUserIdAsync(int userId);
        Task<Customer?> GetByCodeAsync(string customerCode);
        Task<bool> ExistsPhoneAsync(string phone, int? excludedCustomerId = null);
        Task<bool> ExistsEmailAsync(string email, int? excludedCustomerId = null);
        Task<bool> ExistsCodeAsync(string customerCode, int? excludedCustomerId = null);
        Task<bool> HasOperationalReferencesAsync(int customerId);
    }
}
