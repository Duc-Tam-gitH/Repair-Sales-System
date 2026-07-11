namespace R_SS.BLL.DTOs.TechnicalOrder;

public class UsedComponentRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}
