using R_SS.BLL.DTOs.Cart;

namespace R_SS.BLL.Interfaces;

public interface ICartService
{
    /// <summary>
    /// Gets the customer's persisted cart.
    /// </summary>
    Task<CartResponse> GetCartAsync(int customerId);

    /// <summary>
    /// Adds a product to the customer's persisted cart or updates its quantity.
    /// </summary>
    Task<CartResponse> AddToCartAsync(AddToCartRequest request);
}
