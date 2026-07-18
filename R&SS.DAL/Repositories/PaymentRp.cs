using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class PaymentRp : GenericRp<Payment>, IPaymentRp
    {
        public PaymentRp(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<Payment>> GetAllWithDetailsAsync()
        {
            return await _context.Payments
                .AsNoTracking()
                .Include(payment => payment.Customer)
                .Include(payment => payment.SalesOrder)
                .Include(payment => payment.RepairOrder)
                .OrderByDescending(payment => payment.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment?> GetWithDetailsAsync(int paymentId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Include(payment => payment.Customer)
                .Include(payment => payment.SalesOrder)
                .Include(payment => payment.RepairOrder)
                .FirstOrDefaultAsync(payment => payment.PaymentId == paymentId);
        }
    }
}
