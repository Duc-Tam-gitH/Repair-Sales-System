using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IInventoryTransactionRp : IGenericRp<InventoryTransaction>
    {
        Task<IReadOnlyCollection<InventoryTransaction>> SearchAsync(int? productId, DateTime? fromUtc, DateTime? toUtc, string? transactionType);
    }
}
