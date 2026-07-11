namespace R_SS.BLL.DTOs.Order;

public class OrderResponse
{
    public int SalesOrderId { get; set; }
    public string SalesOrderCode { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public IReadOnlyCollection<OrderItemResponse> Items { get; set; } = Array.Empty<OrderItemResponse>();
    public string Message { get; set; } = string.Empty;
}
