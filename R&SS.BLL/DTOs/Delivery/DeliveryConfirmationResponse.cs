namespace R_SS.BLL.DTOs.Delivery;

public class DeliveryConfirmationResponse
{
    public int RepairOrderId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DeliveryConfirmedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
