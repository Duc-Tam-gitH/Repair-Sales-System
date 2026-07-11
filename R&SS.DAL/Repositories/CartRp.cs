using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class CartRp : GenericRp<Cart>, ICartRp
    {
        public CartRp(AppDbContext context) : base(context)
        {
        }

        public async Task<Cart?> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Carts
                .Include(cart => cart.CartItems)
                .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(cart => cart.CustomerId == customerId);
        }
    }
}
