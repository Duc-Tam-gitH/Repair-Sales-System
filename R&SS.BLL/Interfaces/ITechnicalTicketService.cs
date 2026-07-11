using R_SS.BLL.DTOs.TechnicalOrder;

namespace R_SS.BLL.Interfaces;

public interface ITechnicalTicketService
{
    Task<TechnicalTicketResponse> CreateAsync(CreateTechnicalTicketRequest request);
    Task<TechnicalTicketListResponse> GetTicketsAsync(ViewTechnicalTicketsRequest request);
    Task<TechnicalTicketResponse> GetDetailsAsync(int repairOrderId, ViewTechnicalTicketsRequest viewer);
    Task<TechnicalTicketProgressResponse> TrackProgressAsync(int repairOrderId, ViewTechnicalTicketsRequest viewer);
    Task<TechnicianListResponse> GetTechniciansAsync(string actorRole);
    Task<TechnicalTicketResponse> AssignTechnicianAsync(AssignTechnicianRequest request);
    Task<TechnicalTicketResponse> UpdateAsync(UpdateTechnicalTicketRequest request);
    Task<TechnicalTicketResponse> ConfirmPaymentAsync(ConfirmRepairPaymentRequest request);
    Task<TechnicalTicketResponse> CancelAsync(CancelTechnicalTicketRequest request);
}
