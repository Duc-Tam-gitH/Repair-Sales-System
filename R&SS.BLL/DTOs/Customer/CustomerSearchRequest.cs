namespace R_SS.BLL.DTOs.Customer;

public class CustomerSearchRequest
{
    public string? Keyword { get; set; }
    public string ActorRole { get; set; } = string.Empty;
}
