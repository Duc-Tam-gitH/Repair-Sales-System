namespace R_SS.BLL.DTOs.Customer;

public class CustomerSummaryResponse
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}
