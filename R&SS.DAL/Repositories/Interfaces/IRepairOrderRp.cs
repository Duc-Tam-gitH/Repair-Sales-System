using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IRepairOrderRp : IGenericRp<RepairOrder>
    {
        Task<IReadOnlyCollection<RepairOrder>> GetSubmittedByCustomerIdAsync(int customerId, bool includeCanceled);
        Task<RepairOrder?> GetWithDetailsAsync(int repairOrderId);
        Task<IReadOnlyCollection<RepairOrder>> GetVisibleTicketsAsync(string actorRole, int actorUserId, int? customerId);
        Task<int> CountActiveByTechnicianAsync(int technicianId);
        Task<bool> ExistsCodeAsync(string repairOrderCode);
    }
}
