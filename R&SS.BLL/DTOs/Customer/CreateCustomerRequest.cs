namespace R_SS.BLL.DTOs.Customer;

public class CreateCustomerRequest
{
    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public int ActorUserId { get; set; }
}
