using R_SS.BLL.DTOs.Inventory;

namespace R_SS.BLL.Interfaces;

public interface IInventoryStatisticService
{
    Task<InventoryStatisticResponse> GenerateAsync(InventoryStatisticRequest request);
}
