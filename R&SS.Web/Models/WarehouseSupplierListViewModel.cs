using R_SS.BLL.DTOs.Supplier;

namespace R_SS.Web.Models;

public class WarehouseSupplierListViewModel
{
    public IReadOnlyCollection<SupplierResponse> Suppliers { get; set; } = Array.Empty<SupplierResponse>();
    public ManageSupplierRequest Supplier { get; set; } = new();
    public string? Keyword { get; set; }
}
