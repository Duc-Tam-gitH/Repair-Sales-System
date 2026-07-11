using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class ServiceFeedbackRp : GenericRp<ServiceFeedback>, IServiceFeedbackRp
    {
        public ServiceFeedbackRp(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsForSalesOrderAsync(int salesOrderId)
        {
            return await _context.ServiceFeedbacks.AnyAsync(feedback => feedback.SalesOrderId == salesOrderId);
        }

        public async Task<bool> ExistsForRepairOrderAsync(int repairOrderId)
        {
            return await _context.ServiceFeedbacks.AnyAsync(feedback => feedback.RepairOrderId == repairOrderId);
        }

        public async Task<IReadOnlyCollection<ServiceFeedback>> GetAllWithReferencesAsync()
        {
            return await _context.ServiceFeedbacks
                .AsNoTracking()
                .Include(feedback => feedback.SalesOrder)
                .Include(feedback => feedback.RepairOrder)
                .ToListAsync();
        }
    }
}
