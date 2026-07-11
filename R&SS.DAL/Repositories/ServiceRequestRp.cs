using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class ServiceRequestRp : GenericRp<ServiceRequest>, IServiceRequestRp
    {
        public ServiceRequestRp(AppDbContext context) : base(context) { }
        public async Task<IReadOnlyCollection<ServiceRequest>> GetByCustomerIdAsync(int customerId) => await _context.ServiceRequests.AsNoTracking().Where(x => x.CustomerId == customerId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync();
        public async Task<IReadOnlyCollection<ServiceRequest>> GetPendingReceptionAsync() => await _context.ServiceRequests.AsNoTracking().Where(x => x.Status == "Pending Reception").OrderBy(x => x.CreatedAtUtc).ToListAsync();
        public async Task<bool> ExistsCodeAsync(string requestCode) => await _context.ServiceRequests.AnyAsync(x => x.RequestCode == requestCode);
    }
}
