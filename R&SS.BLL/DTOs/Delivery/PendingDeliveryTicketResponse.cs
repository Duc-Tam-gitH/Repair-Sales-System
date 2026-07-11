namespace R_SS.BLL.DTOs.Delivery;

public class PendingDeliveryTicketResponse
{
    public int RepairOrderId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public DateTime? DeadlineUtc { get; set; }
}
