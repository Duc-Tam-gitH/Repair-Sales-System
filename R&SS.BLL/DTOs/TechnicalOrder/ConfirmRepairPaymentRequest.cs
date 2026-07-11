namespace R_SS.BLL.DTOs.TechnicalOrder;

public class ConfirmRepairPaymentRequest
{
    public int RepairOrderId { get; set; }
    public int ConfirmedByUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ManualDeliveryNote { get; set; }
}
