namespace R_SS.BLL.DTOs.Product;

public class ManageProductRequest
{
    public int? ProductId { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int ProductCategoryId { get; set; }
    public int? SupplierId { get; set; }
    public decimal SalePrice { get; set; }
    public int QuantityInStock { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
