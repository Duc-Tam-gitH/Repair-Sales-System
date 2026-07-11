using R_SS.BLL.DTOs.ServiceRequest;

namespace R_SS.BLL.Interfaces;

public interface IServiceRequestService
{
    Task<ServiceRequestResponse> SendAsync(SendServiceRequestRequest request);
    Task<ServiceRequestResponse> CancelAsync(CancelServiceRequestRequest request);
    Task<IReadOnlyCollection<ServiceRequestResponse>> GetByCustomerAsync(int customerId);
}
