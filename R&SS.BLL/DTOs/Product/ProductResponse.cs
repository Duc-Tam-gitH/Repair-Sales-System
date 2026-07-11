namespace R_SS.BLL.DTOs.Product;

public class ProductResponse
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public int QuantityInStock { get; set; }
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }
}
