using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class CartItemRp : GenericRp<CartItem>, ICartItemRp
    {
        public CartItemRp(AppDbContext context) : base(context)
        {
        }

        public async Task<CartItem?> GetByCartAndProductAsync(int cartId, int productId)
        {
            return await _context.CartItems
                .Include(item => item.Product)
                .FirstOrDefaultAsync(item => item.CartId == cartId && item.ProductId == productId);
        }

        public async Task<IReadOnlyCollection<CartItem>> GetByCartIdAsync(int cartId)
        {
            return await _context.CartItems
                .Include(item => item.Product)
                .Where(item => item.CartId == cartId)
                .ToListAsync();
        }

        public void DeleteRange(IEnumerable<CartItem> items)
        {
            _context.CartItems.RemoveRange(items);
        }
    }
}
