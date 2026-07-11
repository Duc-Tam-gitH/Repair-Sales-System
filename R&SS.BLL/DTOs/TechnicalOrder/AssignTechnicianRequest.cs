namespace R_SS.BLL.DTOs.TechnicalOrder;

public class AssignTechnicianRequest
{
    public int RepairOrderId { get; set; }
    public int TechnicianId { get; set; }
    public int AssignedByUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
}
