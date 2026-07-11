namespace R_SS.BLL.DTOs.Inventory;

public class InventoryStatisticResponse
{
    public IReadOnlyCollection<InventoryStatisticItemResponse> Items { get; set; } = Array.Empty<InventoryStatisticItemResponse>();
    public string Message { get; set; } = string.Empty;
}
