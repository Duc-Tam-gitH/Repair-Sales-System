namespace R_SS.BLL.DTOs.TechnicalOrder;

public class ViewTechnicalTicketsRequest
{
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
}
