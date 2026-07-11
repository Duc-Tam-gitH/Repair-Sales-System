namespace R_SS.BLL.DTOs.Inventory;

public class InventoryTransactionRequest
{
    public int ProductId { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Reason { get; set; }
}
