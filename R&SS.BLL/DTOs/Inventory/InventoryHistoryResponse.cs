namespace R_SS.BLL.DTOs.Inventory;

public class InventoryHistoryResponse
{
    public IReadOnlyCollection<InventoryTransactionResponse> Transactions { get; set; } = Array.Empty<InventoryTransactionResponse>();
    public string Message { get; set; } = string.Empty;
}
