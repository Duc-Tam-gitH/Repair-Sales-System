using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class SalesOrderRp : GenericRp<SalesOrder>, ISalesOrderRp
    {
        public SalesOrderRp(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<SalesOrder>> GetAllWithDetailsAsync()
        {
            return await _context.SalesOrders
                .AsNoTracking()
                .Include(order => order.Customer)
                .Include(order => order.SalesOrderDetails)
                .ThenInclude(detail => detail.Product)
                .Include(order => order.Payments)
                .OrderByDescending(order => order.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<SalesOrder>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.SalesOrders
                .AsNoTracking()
                .Include(order => order.SalesOrderDetails)
                .ThenInclude(detail => detail.Product)
                .Where(order => order.CustomerId == customerId)
                .OrderByDescending(order => order.CreatedAt)
                .ToListAsync();
        }

        public async Task<SalesOrder?> GetWithDetailsAsync(int salesOrderId)
        {
            return await _context.SalesOrders
                .Include(order => order.SalesOrderDetails)
                .ThenInclude(detail => detail.Product)
                .Include(order => order.Payments)
                .Include(order => order.Customer)
                .FirstOrDefaultAsync(order => order.SalesOrderId == salesOrderId);
        }

        public async Task<bool> ExistsCodeAsync(string salesOrderCode)
        {
            return await _context.SalesOrders.AnyAsync(order => order.SalesOrderCode == salesOrderCode);
        }
    }
}
