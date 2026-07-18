using R_SS.Models.Entities;

namespace R_SS.Web.Models;

public class ProductOrderListViewModel
{
    public IReadOnlyCollection<SalesOrder> Orders { get; set; } = Array.Empty<SalesOrder>();
    public string? Keyword { get; set; }
    public string? Status { get; set; }
}
