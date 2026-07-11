namespace R_SS.BLL.DTOs.Order;

public class PlaceOrderRequest
{
    public int CustomerId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
}
