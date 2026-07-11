using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface ISalesOrderRp : IGenericRp<SalesOrder>
    {
        Task<IReadOnlyCollection<SalesOrder>> GetByCustomerIdAsync(int customerId);
        Task<SalesOrder?> GetWithDetailsAsync(int salesOrderId);
        Task<bool> ExistsCodeAsync(string salesOrderCode);
    }
}
