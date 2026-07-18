using R_SS.BLL.DTOs.Inventory;
using R_SS.BLL.DTOs.Product;

namespace R_SS.Web.Models;

public class WarehouseProductDetailsViewModel
{
    public ProductDetailResponse Product { get; set; } = new();
    public IReadOnlyCollection<InventoryTransactionResponse> Transactions { get; set; } = Array.Empty<InventoryTransactionResponse>();
}
