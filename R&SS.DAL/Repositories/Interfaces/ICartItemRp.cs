using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface ICartItemRp : IGenericRp<CartItem>
    {
        Task<CartItem?> GetByCartAndProductAsync(int cartId, int productId);
        Task<IReadOnlyCollection<CartItem>> GetByCartIdAsync(int cartId);
        void DeleteRange(IEnumerable<CartItem> items);
    }
}
