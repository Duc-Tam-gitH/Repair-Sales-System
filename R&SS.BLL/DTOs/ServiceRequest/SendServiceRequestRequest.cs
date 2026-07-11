namespace R_SS.BLL.DTOs.ServiceRequest;

public class SendServiceRequestRequest
{
    public int CustomerId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? DeviceModel { get; set; }
    public string Description { get; set; } = string.Empty;
    public IReadOnlyCollection<ServiceRequestImageRequest> Images { get; set; } = Array.Empty<ServiceRequestImageRequest>();
}
