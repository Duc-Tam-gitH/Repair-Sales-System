using R_SS.BLL.DTOs.Order;

namespace R_SS.BLL.Interfaces;

public interface IOrderService
{
    /// <summary>
    /// Places an order from the customer's cart.
    /// </summary>
    Task<OrderResponse> PlaceOrderAsync(PlaceOrderRequest request);

    /// <summary>
    /// Processes an in-store sales order for staff.
    /// </summary>
    Task<OrderResponse> ProcessSalesOrderAsync(ProcessSalesOrderRequest request);

    Task<OrderResponse> CancelOrderAsync(CancelOrderRequest request);
}
