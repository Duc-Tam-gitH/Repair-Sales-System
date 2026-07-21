using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class CustomerRp : GenericRp<Customer>, ICustomerRp
    {
        private const string CustomerRoleName = "Customer";

        public CustomerRp(AppDbContext context) : base(context)
        {
        }

        public new async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await ApplyCurrentCustomerRoleFilter(_context.Customers.AsNoTracking())
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<Customer>> SearchAsync(string? keyword)
        {
            var query = ApplyCurrentCustomerRoleFilter(_context.Customers.AsNoTracking());

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var normalizedKeyword = keyword.Trim().ToLower();
                query = query.Where(customer =>
                    customer.FullName.ToLower().Contains(normalizedKeyword) ||
                    customer.CustomerCode.ToLower().Contains(normalizedKeyword) ||
                    (customer.Phone != null && customer.Phone.ToLower().Contains(normalizedKeyword)) ||
                    (customer.Email != null && customer.Email.ToLower().Contains(normalizedKeyword)));
            }

            return await query
                .OrderBy(customer => customer.FullName)
                .ToListAsync();
        }

        public async Task<Customer?> GetByUserIdAsync(int userId)
        {
            return await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(customer => customer.UserId == userId);
        }

        public async Task<Customer?> GetByCodeAsync(string customerCode)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(customer => customer.CustomerCode.ToLower() == customerCode.ToLower());
        }

        public async Task<bool> ExistsPhoneAsync(string phone, int? excludedCustomerId = null)
        {
            return await _context.Customers.AnyAsync(customer =>
                customer.Phone != null &&
                customer.Phone.ToLower() == phone.ToLower() &&
                (!excludedCustomerId.HasValue || customer.CustomerId != excludedCustomerId.Value));
        }

        public async Task<bool> ExistsEmailAsync(string email, int? excludedCustomerId = null)
        {
            return await _context.Customers.AnyAsync(customer =>
                customer.Email != null &&
                customer.Email.ToLower() == email.ToLower() &&
                (!excludedCustomerId.HasValue || customer.CustomerId != excludedCustomerId.Value));
        }

        public async Task<bool> ExistsCodeAsync(string customerCode, int? excludedCustomerId = null)
        {
            return await _context.Customers.AnyAsync(customer =>
                customer.CustomerCode.ToLower() == customerCode.ToLower() &&
                (!excludedCustomerId.HasValue || customer.CustomerId != excludedCustomerId.Value));
        }

        public async Task<bool> HasOperationalReferencesAsync(int customerId)
        {
            return await _context.SalesOrders.AnyAsync(order => order.CustomerId == customerId) ||
                await _context.RepairOrders.AnyAsync(order => order.CustomerId == customerId) ||
                await _context.Payments.AnyAsync(payment => payment.CustomerId == customerId) ||
                await _context.ServiceRequests.AnyAsync(request => request.CustomerId == customerId);
        }

        private static IQueryable<Customer> ApplyCurrentCustomerRoleFilter(IQueryable<Customer> query)
        {
            return query.Where(customer =>
                !customer.UserId.HasValue ||
                (customer.User != null &&
                    customer.User.UserRoles.Any(userRole =>
                        userRole.Role != null &&
                        userRole.Role.RoleName == CustomerRoleName)));
        }
    }
}
