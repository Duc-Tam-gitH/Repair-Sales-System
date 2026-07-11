using R_SS.BLL.DTOs.Inventory;

namespace R_SS.BLL.Interfaces;

public interface IInventoryService
{
    Task<InventoryTransactionResponse> ApplyTransactionAsync(InventoryTransactionRequest request);
    Task<InventoryHistoryResponse> GetHistoryAsync(InventoryHistoryRequest request);
}
