namespace R_SS.BLL.DTOs.Customer;

public class UpdateCustomerStatusRequest
{
    public int CustomerId { get; set; }
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public int ActorUserId { get; set; }
}
