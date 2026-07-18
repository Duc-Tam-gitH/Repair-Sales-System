using R_SS.BLL.DTOs.Activity;
using R_SS.BLL.DTOs.Configuration;

namespace R_SS.Web.Models;

public class AuditLogViewModel
{
    public SystemActivityLogListResponse Logs { get; set; } = new();
    public SystemActivityLogSearchRequest Search { get; set; } = new();
}

public class ProcessManagementViewModel
{
    public IReadOnlyCollection<SystemConfigurationResponse> Configurations { get; set; } = Array.Empty<SystemConfigurationResponse>();
    public UpdateSystemConfigurationRequest Configuration { get; set; } = new();
}
