using R_SS.BLL.DTOs.Inventory;
using R_SS.BLL.DTOs.Product;

namespace R_SS.Web.Models;

public class WarehouseInventoryViewModel
{
    public IReadOnlyCollection<ProductResponse> Products { get; set; } = Array.Empty<ProductResponse>();
    public IReadOnlyCollection<InventoryTransactionResponse> Transactions { get; set; } = Array.Empty<InventoryTransactionResponse>();
    public InventoryTransactionRequest Transaction { get; set; } = new();
}
