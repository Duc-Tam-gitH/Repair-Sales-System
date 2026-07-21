using R_SS.BLL.DTOs.Customer;
using R_SS.BLL.DTOs.ServiceRequest;
using R_SS.BLL.DTOs.TechnicalOrder;

namespace R_SS.Web.Models;

public class ReceptionIntakeViewModel
{
    public IReadOnlyCollection<CustomerSummaryResponse> Customers { get; set; } = Array.Empty<CustomerSummaryResponse>();
    public IReadOnlyCollection<TechnicianWorkloadResponse> Technicians { get; set; } = Array.Empty<TechnicianWorkloadResponse>();
    public ServiceRequestResponse? SourceRequest { get; set; }
    public CreateTechnicalTicketRequest Ticket { get; set; } = new();
}
