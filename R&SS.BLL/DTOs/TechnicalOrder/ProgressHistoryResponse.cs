namespace R_SS.BLL.DTOs.TechnicalOrder;

public class ProgressHistoryResponse
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
