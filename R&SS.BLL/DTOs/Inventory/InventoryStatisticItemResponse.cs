namespace R_SS.BLL.DTOs.Inventory;

public class InventoryStatisticItemResponse
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int ReceiptQuantity { get; set; }
    public int IssueQuantity { get; set; }
    public int StockQuantity { get; set; }
    public string InventoryStatus { get; set; } = string.Empty;
}
