namespace R_SS.BLL.DTOs.Cart;

public class CartResponse
{
    public int CartId { get; set; }
    public int CustomerId { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public IReadOnlyCollection<CartItemResponse> Items { get; set; } = Array.Empty<CartItemResponse>();
    public string Message { get; set; } = string.Empty;
}
