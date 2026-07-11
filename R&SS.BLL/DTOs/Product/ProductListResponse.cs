namespace R_SS.BLL.DTOs.Product;

public class ProductListResponse
{
    public IReadOnlyCollection<ProductResponse> Products { get; set; } = Array.Empty<ProductResponse>();
    public string Message { get; set; } = string.Empty;
}
