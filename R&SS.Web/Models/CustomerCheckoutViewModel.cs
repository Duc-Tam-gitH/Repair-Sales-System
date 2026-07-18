using R_SS.BLL.DTOs.Cart;
using R_SS.BLL.DTOs.Order;

namespace R_SS.Web.Models;

public class CustomerCheckoutViewModel
{
    public CartResponse Cart { get; set; } = new();
    public PlaceOrderRequest Order { get; set; } = new();
}
