namespace R_SS.BLL.DTOs.ServiceRequest;

public class ServiceRequestResponse
{
    public int ServiceRequestId { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? DeviceModel { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool NeedsManualProcessing { get; set; }
    public IReadOnlyCollection<string> ImageFileNames { get; set; } = Array.Empty<string>();
    public string Message { get; set; } = string.Empty;
}
