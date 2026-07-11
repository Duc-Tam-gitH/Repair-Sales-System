using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IServiceRequestRp : IGenericRp<ServiceRequest>
    {
        Task<IReadOnlyCollection<ServiceRequest>> GetByCustomerIdAsync(int customerId);
        Task<IReadOnlyCollection<ServiceRequest>> GetPendingReceptionAsync();
        Task<bool> ExistsCodeAsync(string requestCode);
    }
}
