using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class RepairOrderRp : GenericRp<RepairOrder>, IRepairOrderRp
    {
        public RepairOrderRp(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<RepairOrder>> GetSubmittedByCustomerIdAsync(int customerId, bool includeCanceled)
        {
            var query = _context.RepairOrders
                .AsNoTracking()
                .Include(order => order.RepairOrderDetails)
                .ThenInclude(detail => detail.Product)
                .Where(order => order.CustomerId == customerId);

            if (!includeCanceled)
            {
                query = query.Where(order => order.Status.ToLower() != "canceled");
            }

            return await query
                .OrderByDescending(order => order.CreatedAt)
                .ToListAsync();
        }

        public async Task<RepairOrder?> GetWithDetailsAsync(int repairOrderId)
        {
            return await _context.RepairOrders
                .Include(order => order.RepairOrderDetails)
                .ThenInclude(detail => detail.Product)
                .Include(order => order.Payments)
                .Include(order => order.Customer)
                .Include(order => order.AssignedTechnician)
                .Include(order => order.StatusHistories)
                .Include(order => order.AssignmentHistories)
                .FirstOrDefaultAsync(order => order.RepairOrderId == repairOrderId);
        }

        public async Task<IReadOnlyCollection<RepairOrder>> GetVisibleTicketsAsync(string actorRole, int actorUserId, int? customerId)
        {
            var normalizedRole = actorRole.Trim().ToLower();
            var query = _context.RepairOrders
                .AsNoTracking()
                .Include(order => order.Customer)
                .Include(order => order.AssignedTechnician)
                .Include(order => order.StatusHistories)
                .AsQueryable();

            query = normalizedRole switch
            {
                "customer" => customerId.HasValue
                    ? query.Where(order => order.CustomerId == customerId.Value)
                    : query.Where(order => false),
                "technician" => query.Where(order => order.AssignedTechnicianId == actorUserId),
                "manager" => query,
                "receptionist" => query,
                _ => query.Where(order => false)
            };

            return await query.OrderByDescending(order => order.CreatedAt).ToListAsync();
        }

        public async Task<int> CountActiveByTechnicianAsync(int technicianId)
        {
            var closedStatuses = new[] { "completed", "delivered", "canceled" };
            return await _context.RepairOrders.CountAsync(order =>
                order.AssignedTechnicianId == technicianId &&
                !closedStatuses.Contains(order.Status.ToLower()));
        }

        public async Task<bool> ExistsCodeAsync(string repairOrderCode)
        {
            return await _context.RepairOrders.AnyAsync(order => order.RepairOrderCode == repairOrderCode);
        }
    }
}
