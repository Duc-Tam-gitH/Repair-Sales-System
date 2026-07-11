using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class CustomerRp : GenericRp<Customer>, ICustomerRp
    {
        public CustomerRp(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyCollection<Customer>> SearchAsync(string? keyword)
        {
            var query = _context.Customers.AsNoTracking().AsQueryable();

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
    }
}
