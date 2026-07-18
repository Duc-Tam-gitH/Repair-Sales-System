using R_SS.BLL.DTOs.ProductCategory;
using R_SS.Models.Entities;

namespace R_SS.Web.Models;

public class ProductCategoryManagementViewModel
{
    public IReadOnlyCollection<ProductCategory> Categories { get; set; } = Array.Empty<ProductCategory>();
    public ManageProductCategoryRequest Category { get; set; } = new();
    public string? Keyword { get; set; }
}
