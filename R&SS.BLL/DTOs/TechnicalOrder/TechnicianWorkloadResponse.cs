namespace R_SS.BLL.DTOs.TechnicalOrder;

public class TechnicianWorkloadResponse
{
    public int TechnicianId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string WorkStatus { get; set; } = string.Empty;
    public int ActiveTicketCount { get; set; }
}
