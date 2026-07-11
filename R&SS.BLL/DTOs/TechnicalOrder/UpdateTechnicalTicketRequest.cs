namespace R_SS.BLL.DTOs.TechnicalOrder;

public class UpdateTechnicalTicketRequest
{
    public int RepairOrderId { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string? CustomerFullName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public string? InspectionResult { get; set; }
    public string? Diagnosis { get; set; }
    public string? DeviceCondition { get; set; }
    public string? WorkPerformed { get; set; }
    public string? RepairResult { get; set; }
    public string? AccompanyingAccessories { get; set; }
    public int? ProcessingMinutes { get; set; }
    public decimal? ServiceFee { get; set; }
    public string? StatusDecision { get; set; }
    public IReadOnlyCollection<UsedComponentRequest> UsedComponents { get; set; } = Array.Empty<UsedComponentRequest>();
}
