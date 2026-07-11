namespace R_SS.BLL.DTOs.Report;

public class RevenueReportRequest
{
    public string ActorRole { get; set; } = string.Empty;
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public string RevenueType { get; set; } = "all";
    public string ExportFormat { get; set; } = "none";
}
