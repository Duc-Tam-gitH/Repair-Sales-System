namespace R_SS.BLL.DTOs.ProductCategory;

public class ProductCategoryResponse
{
    public int ProductCategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string Message { get; set; } = string.Empty;
}
