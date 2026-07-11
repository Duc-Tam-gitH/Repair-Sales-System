namespace R_SS.BLL.DTOs.TechnicalOrder;

public class TechnicianListResponse
{
    public IReadOnlyCollection<TechnicianWorkloadResponse> Technicians { get; set; } = Array.Empty<TechnicianWorkloadResponse>();
    public string Message { get; set; } = string.Empty;
}
