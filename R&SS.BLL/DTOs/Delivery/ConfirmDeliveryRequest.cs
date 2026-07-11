namespace R_SS.BLL.DTOs.Delivery;

public class ConfirmDeliveryRequest
{
    public int RepairOrderId { get; set; }
    public int CustomerId { get; set; }
    public string OtpCode { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
}
