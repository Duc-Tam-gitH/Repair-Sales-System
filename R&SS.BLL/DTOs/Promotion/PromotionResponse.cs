namespace R_SS.BLL.DTOs.Promotion;

public class PromotionResponse
{
    public int PromotionId { get; set; }
    public string PromotionCode { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string PromotionType { get; set; } = string.Empty;
    public decimal PromotionValue { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyCollection<int> ProductIds { get; set; } = Array.Empty<int>();
    public string Message { get; set; } = string.Empty;
}
