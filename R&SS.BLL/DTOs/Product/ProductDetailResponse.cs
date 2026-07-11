namespace R_SS.BLL.DTOs.Product;

public class ProductDetailResponse : ProductResponse
{
    public string? Description { get; set; }
    public string Message { get; set; } = string.Empty;
}
