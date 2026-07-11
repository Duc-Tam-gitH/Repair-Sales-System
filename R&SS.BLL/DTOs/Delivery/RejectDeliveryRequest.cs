namespace R_SS.BLL.DTOs.Delivery;

public class RejectDeliveryRequest
{
    public int RepairOrderId { get; set; }
    public int CustomerId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
