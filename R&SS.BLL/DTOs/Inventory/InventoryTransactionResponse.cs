namespace R_SS.BLL.DTOs.Inventory;

public class InventoryTransactionResponse
{
    public int InventoryTransactionId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public int QuantityChange { get; set; }
    public int StockBefore { get; set; }
    public int StockAfter { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
