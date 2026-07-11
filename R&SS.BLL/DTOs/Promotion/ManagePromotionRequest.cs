namespace R_SS.BLL.DTOs.Promotion;

public class ManagePromotionRequest
{
    public int? PromotionId { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string PromotionType { get; set; } = string.Empty;
    public decimal PromotionValue { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyCollection<int> ProductIds { get; set; } = Array.Empty<int>();
}
