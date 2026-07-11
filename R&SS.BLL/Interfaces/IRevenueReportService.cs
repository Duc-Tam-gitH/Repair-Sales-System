using R_SS.BLL.DTOs.Report;

namespace R_SS.BLL.Interfaces;

public interface IRevenueReportService
{
    Task<RevenueReportResponse> GenerateAsync(RevenueReportRequest request);
}
