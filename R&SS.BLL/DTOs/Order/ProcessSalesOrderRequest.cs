namespace R_SS.BLL.DTOs.Order;

public class ProcessSalesOrderRequest
{
    public int CustomerId { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public IReadOnlyCollection<OrderItemRequest> Items { get; set; } = Array.Empty<OrderItemRequest>();
}
