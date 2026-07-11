namespace R_SS.BLL.DTOs.Product;

public class SearchProductsRequest
{
    public string Keyword { get; set; } = string.Empty;
    public string Criteria { get; set; } = "all";
}
