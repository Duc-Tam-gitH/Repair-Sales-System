namespace R_SS.BLL.DTOs.TechnicalOrder;

public class TechnicalTicketProgressResponse
{
    public TechnicalTicketResponse Ticket { get; set; } = new();
    public IReadOnlyCollection<ProgressHistoryResponse> History { get; set; } = Array.Empty<ProgressHistoryResponse>();
    public string Message { get; set; } = string.Empty;
}
