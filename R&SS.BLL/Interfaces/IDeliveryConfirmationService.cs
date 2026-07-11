using R_SS.BLL.DTOs.Delivery;

namespace R_SS.BLL.Interfaces;

public interface IDeliveryConfirmationService
{
    Task<IReadOnlyCollection<PendingDeliveryTicketResponse>> GetPendingAsync(int customerId);
    Task<DeliveryConfirmationResponse> ConfirmAsync(ConfirmDeliveryRequest request);
    Task<DeliveryConfirmationResponse> RejectAsync(RejectDeliveryRequest request);
    Task<DeliveryConfirmationResponse> ResendOtpAsync(ResendDeliveryOtpRequest request);
}
