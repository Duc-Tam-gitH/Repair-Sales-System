namespace R_SS.BLL.DTOs.TechnicalOrder;

public class CancelTechnicalTicketRequest
{
    public int RepairOrderId { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
