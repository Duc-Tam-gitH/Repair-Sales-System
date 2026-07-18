using R_SS.BLL.DTOs.Product;
using R_SS.BLL.DTOs.Supplier;
using R_SS.Models.Entities;

namespace R_SS.Web.Models;

public class ProductCatalogViewModel
{
    public IReadOnlyCollection<ProductResponse> Products { get; set; } = Array.Empty<ProductResponse>();
    public IReadOnlyCollection<ProductCategory> Categories { get; set; } = Array.Empty<ProductCategory>();
    public IReadOnlyCollection<SupplierResponse> Suppliers { get; set; } = Array.Empty<SupplierResponse>();
    public ManageProductRequest Product { get; set; } = new();
    public string? Keyword { get; set; }
}
