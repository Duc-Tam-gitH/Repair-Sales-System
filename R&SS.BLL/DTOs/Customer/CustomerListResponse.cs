namespace R_SS.BLL.DTOs.Customer;

public class CustomerListResponse
{
    public IReadOnlyCollection<CustomerSummaryResponse> Customers { get; set; } = Array.Empty<CustomerSummaryResponse>();
    public string Message { get; set; } = string.Empty;
}
