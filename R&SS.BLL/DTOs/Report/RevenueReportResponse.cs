namespace R_SS.BLL.DTOs.Report;

public class RevenueReportResponse
{
    public decimal TotalRevenue { get; set; }
    public decimal ProductSalesRevenue { get; set; }
    public decimal RepairServiceRevenue { get; set; }
    public int TransactionCount { get; set; }
    public string? ExportFileName { get; set; }
    public string Message { get; set; } = string.Empty;
}
