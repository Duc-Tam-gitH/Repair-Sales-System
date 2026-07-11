namespace R_SS.BLL.DTOs.Inventory;

public class InventoryHistoryRequest
{
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public int? ProductId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public string? TransactionType { get; set; }
}
