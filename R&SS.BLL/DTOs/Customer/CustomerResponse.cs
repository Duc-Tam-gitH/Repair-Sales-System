namespace R_SS.BLL.DTOs.Customer;

public class CustomerResponse
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public string Message { get; set; } = string.Empty;
}
