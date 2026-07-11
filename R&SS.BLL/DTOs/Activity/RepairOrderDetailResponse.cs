namespace R_SS.BLL.DTOs.Activity;

public class RepairOrderDetailResponse
{
    public int RepairOrderId { get; set; }
    public string RepairOrderCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string IssueDescription { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Message { get; set; } = string.Empty;
}
