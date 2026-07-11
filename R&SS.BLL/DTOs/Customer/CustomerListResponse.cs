namespace R_SS.BLL.DTOs.Customer;

public class CustomerListResponse
{
    public IReadOnlyCollection<CustomerResponse> Customers { get; set; } = Array.Empty<CustomerResponse>();
    public string Message { get; set; } = string.Empty;
}
