namespace R_SS.BLL.DTOs.TechnicalOrder;

public class TechnicalTicketListResponse
{
    public IReadOnlyCollection<TechnicalTicketResponse> Tickets { get; set; } = Array.Empty<TechnicalTicketResponse>();
    public string Message { get; set; } = string.Empty;
}
