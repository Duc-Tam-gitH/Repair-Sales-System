using R_SS.BLL.DTOs.Activity;
using R_SS.BLL.DTOs.Order;

namespace R_SS.BLL.Interfaces;

public interface IActivityHistoryService
{
    /// <summary>
    /// Gets order and technical service activity history for a customer.
    /// </summary>
    Task<ActivityHistoryResponse> GetHistoryAsync(ActivityHistoryRequest request);

    /// <summary>
    /// Gets a sales order detail owned by the customer.
    /// </summary>
    Task<OrderResponse> GetSalesOrderDetailsAsync(int customerId, int salesOrderId);

    /// <summary>
    /// Gets a repair order detail owned by the customer.
    /// </summary>
    Task<RepairOrderDetailResponse> GetRepairOrderDetailsAsync(int customerId, int repairOrderId);
}
