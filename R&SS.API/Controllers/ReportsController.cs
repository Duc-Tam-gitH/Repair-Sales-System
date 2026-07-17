using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Report;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize(Roles = "Manager")]
[Route("api/reports")]
public class ReportsController : AuthenticatedControllerBase
{
    private readonly IRevenueReportService _revenueReportService;

    public ReportsController(IRevenueReportService revenueReportService)
    {
        _revenueReportService = revenueReportService;
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] RevenueReportRequest request)
    {
        request.ActorRole = CurrentRole();
        var result = await _revenueReportService.GenerateAsync(request);
        return Ok(new ApiResponse<RevenueReportResponse> { Success = true, Message = result.Message, Data = result });
    }
}
