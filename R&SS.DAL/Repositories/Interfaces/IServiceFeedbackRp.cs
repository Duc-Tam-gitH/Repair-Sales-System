using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IServiceFeedbackRp : IGenericRp<ServiceFeedback>
    {
        Task<bool> ExistsForSalesOrderAsync(int salesOrderId);
        Task<bool> ExistsForRepairOrderAsync(int repairOrderId);
        Task<IReadOnlyCollection<ServiceFeedback>> GetAllWithReferencesAsync();
    }
}
