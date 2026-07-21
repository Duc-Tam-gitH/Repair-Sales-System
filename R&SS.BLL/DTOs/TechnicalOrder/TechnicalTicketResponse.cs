namespace R_SS.BLL.DTOs.TechnicalOrder;

public class TechnicalTicketResponse
{
    public int RepairOrderId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? DeviceModel { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string IssueDescription { get; set; } = string.Empty;
    public string? DeviceCondition { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? AssignedTechnicianId { get; set; }
    public string? AssignedTechnicianName { get; set; }
    public bool IsHandedOverToTechnician { get; set; }
    public DateTime? DeliveryConfirmationDeadlineUtc { get; set; }
    public bool ShowConfirmDeliveryButton { get; set; }
    public string? DeliveryOtpSentToEmail { get; set; }
    public string? DeliveryOtpSentToPhone { get; set; }
    public DateTime? DeliveryOtpSentAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
