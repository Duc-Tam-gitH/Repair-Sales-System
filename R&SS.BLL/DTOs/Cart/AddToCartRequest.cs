namespace R_SS.BLL.DTOs.Cart;

public class AddToCartRequest
{
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
