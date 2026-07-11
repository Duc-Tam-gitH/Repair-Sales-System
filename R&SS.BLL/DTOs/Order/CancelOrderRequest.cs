namespace R_SS.BLL.DTOs.Order;

public class CancelOrderRequest
{
    public int SalesOrderId { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
