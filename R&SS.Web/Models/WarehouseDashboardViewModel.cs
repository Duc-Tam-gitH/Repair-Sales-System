using R_SS.BLL.DTOs.Inventory;
using R_SS.BLL.DTOs.Product;

namespace R_SS.Web.Models;

public class WarehouseDashboardViewModel
{
    public IReadOnlyCollection<ProductResponse> Products { get; set; } = Array.Empty<ProductResponse>();
    public IReadOnlyCollection<InventoryTransactionResponse> RecentTransactions { get; set; } = Array.Empty<InventoryTransactionResponse>();
    public int TotalStockQuantity { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public decimal EstimatedInventoryValue { get; set; }
}
