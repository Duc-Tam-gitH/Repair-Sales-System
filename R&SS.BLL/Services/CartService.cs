using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.DTOs.Cart;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AddToCartRequest> _validator;
    private readonly ILogger<CartService> _logger;

    public CartService(
        IUnitOfWork unitOfWork,
        IValidator<AddToCartRequest> validator,
        ILogger<CartService> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Gets the customer's persisted cart.
    /// </summary>
    public async Task<CartResponse> GetCartAsync(int customerId)
    {
        if (customerId <= 0)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(customerId), "Customer id must be greater than 0.") });
        }

        var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(customerId);
        if (cart is null)
        {
            return new CartResponse
            {
                CustomerId = customerId,
                Message = "Cart is empty."
            };
        }

        return BuildCartResponse(cart, "Cart retrieved successfully.");
    }

    /// <summary>
    /// Adds a product to the customer's persisted cart or updates its quantity.
    /// </summary>
    public async Task<CartResponse> AddToCartAsync(AddToCartRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _validator.ValidateAsync(request);
        ThrowIfInvalid(validationResult);

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
        if (customer is null)
        {
            throw new NotFoundException("Customer not found.");
        }

        var product = await _unitOfWork.Products.GetActiveProductByIdAsync(request.ProductId);
        if (product is null)
        {
            throw new NotFoundException("Product not found.");
        }

        var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(request.CustomerId);
        if (cart is null)
        {
            cart = new Cart
            {
                CustomerId = request.CustomerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Carts.AddAsync(cart);
        }

        var item = cart.CartId > 0
            ? await _unitOfWork.CartItems.GetByCartAndProductAsync(cart.CartId, request.ProductId)
            : cart.CartItems.FirstOrDefault(cartItem => cartItem.ProductId == request.ProductId);

        var newQuantity = request.Quantity + (item?.Quantity ?? 0);
        if (newQuantity > product.QuantityInStock)
        {
            throw new InvalidOperationException("Stock is insufficient.");
        }

        if (item is null)
        {
            item = new CartItem
            {
                Cart = cart,
                Product = product,
                ProductId = product.ProductId,
                Quantity = request.Quantity,
                UnitPrice = product.SalePrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            cart.CartItems.Add(item);
            await _unitOfWork.CartItems.AddAsync(item);
        }
        else
        {
            item.Quantity = newQuantity;
            item.UnitPrice = product.SalePrice;
            item.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CartItems.Update(item);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Carts.Update(cart);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Customer {CustomerId} added product {ProductId} to cart.", request.CustomerId, request.ProductId);

        return BuildCartResponse(cart, "Product added to cart successfully.");
    }

    private static CartResponse BuildCartResponse(Cart cart, string message)
    {
        var items = cart.CartItems
            .Select(item => new CartItemResponse
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.ProductName ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.UnitPrice * item.Quantity
            })
            .ToArray();

        return new CartResponse
        {
            CartId = cart.CartId,
            CustomerId = cart.CustomerId,
            Items = items,
            TotalQuantity = items.Sum(item => item.Quantity),
            TotalAmount = items.Sum(item => item.LineTotal),
            Message = message
        };
    }

    private static void ThrowIfInvalid(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
