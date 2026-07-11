namespace R_SS.BLL.DTOs.Inventory;

public class InventoryStatisticRequest
{
    public string ActorRole { get; set; } = string.Empty;
    public int? ProductCategoryId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public string StockStatus { get; set; } = "all";
}
