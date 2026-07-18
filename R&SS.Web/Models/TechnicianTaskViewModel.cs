using R_SS.BLL.DTOs.TechnicalOrder;

namespace R_SS.Web.Models;

public class TechnicianTaskViewModel
{
    public TechnicalTicketResponse Ticket { get; set; } = new();
    public IReadOnlyCollection<ProgressHistoryResponse> History { get; set; } = Array.Empty<ProgressHistoryResponse>();
    public UpdateTechnicalTicketRequest Update { get; set; } = new();
}
