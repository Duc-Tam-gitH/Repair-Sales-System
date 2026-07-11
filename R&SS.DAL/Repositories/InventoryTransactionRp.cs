using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class InventoryTransactionRp : GenericRp<InventoryTransaction>, IInventoryTransactionRp
    {
        public InventoryTransactionRp(AppDbContext context) : base(context) { }

        public async Task<IReadOnlyCollection<InventoryTransaction>> SearchAsync(int? productId, DateTime? fromUtc, DateTime? toUtc, string? transactionType)
        {
            var query = _context.InventoryTransactions.AsNoTracking().Include(transaction => transaction.Product).AsQueryable();
            if (productId.HasValue) query = query.Where(transaction => transaction.ProductId == productId.Value);
            if (fromUtc.HasValue) query = query.Where(transaction => transaction.CreatedAtUtc >= fromUtc.Value);
            if (toUtc.HasValue) query = query.Where(transaction => transaction.CreatedAtUtc <= toUtc.Value);
            if (!string.IsNullOrWhiteSpace(transactionType)) query = query.Where(transaction => transaction.TransactionType == transactionType);
            return await query.OrderByDescending(transaction => transaction.CreatedAtUtc).ToListAsync();
        }
    }
}
