using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Cart;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/cart")]
public class CartController : AuthenticatedControllerBase
{
    private readonly ICartService _cartService;
    private readonly IUnitOfWork _unitOfWork;

    public CartController(ICartService cartService, IUnitOfWork unitOfWork)
    {
        _cartService = cartService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var customer = await GetCurrentCustomerAsync();
        var result = await _cartService.GetCartAsync(customer.CustomerId);
        return Ok(new ApiResponse<CartResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (request.CustomerId <= 0)
        {
            request.CustomerId = (await GetCurrentCustomerAsync()).CustomerId;
        }

        var result = await _cartService.AddToCartAsync(request);
        return Ok(new ApiResponse<CartResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPut("items/{itemId:int}")]
    public async Task<IActionResult> UpdateCartItem(int itemId, [FromBody] UpdateCartItemRequest request)
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(customer.CustomerId);
        var item = cart?.CartItems.FirstOrDefault(cartItem => cartItem.CartItemId == itemId);
        if (cart is null || item is null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Cart item not found." });
        }

        if (request.Quantity <= 0)
        {
            _unitOfWork.CartItems.Delete(item);
        }
        else
        {
            item.Quantity = request.Quantity;
            item.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CartItems.Update(item);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Carts.Update(cart);
        await _unitOfWork.SaveChangesAsync();

        var result = await _cartService.GetCartAsync(customer.CustomerId);
        return Ok(new ApiResponse<CartResponse> { Success = true, Message = "Cart item updated successfully.", Data = result });
    }

    [HttpDelete("items/{itemId:int}")]
    public async Task<IActionResult> DeleteCartItem(int itemId)
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(customer.CustomerId);
        var item = cart?.CartItems.FirstOrDefault(cartItem => cartItem.CartItemId == itemId);
        if (cart is null || item is null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Cart item not found." });
        }

        _unitOfWork.CartItems.Delete(item);
        cart.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Carts.Update(cart);
        await _unitOfWork.SaveChangesAsync();

        var result = await _cartService.GetCartAsync(customer.CustomerId);
        return Ok(new ApiResponse<CartResponse> { Success = true, Message = "Cart item deleted successfully.", Data = result });
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(customer.CustomerId);
        if (cart is not null)
        {
            _unitOfWork.CartItems.DeleteRange(cart.CartItems);
            cart.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Carts.Update(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        var result = await _cartService.GetCartAsync(customer.CustomerId);
        return Ok(new ApiResponse<CartResponse> { Success = true, Message = "Cart cleared successfully.", Data = result });
    }

    private async Task<R_SS.Models.Entities.Customer> GetCurrentCustomerAsync()
    {
        var customer = await _unitOfWork.Customers.GetByUserIdAsync(CurrentUserId());
        if (customer is null)
        {
            throw new InvalidOperationException("Customer profile not found.");
        }

        return customer;
    }

    public sealed class UpdateCartItemRequest
    {
        public int Quantity { get; set; }
    }
}
