namespace R_SS.BLL.DTOs.TechnicalOrder;

public class CreateTechnicalTicketRequest
{
    public int CustomerId { get; set; }
    public int ReceivedByUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? DeviceModel { get; set; }
    public string? SerialNumber { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string IssueDescription { get; set; } = string.Empty;
    public string DeviceCondition { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
