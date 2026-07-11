namespace R_SS.BLL.DTOs.Activity;

public class ActivityHistoryRequest
{
    public int CustomerId { get; set; }
    public string Type { get; set; } = "all";
    public bool IncludeCanceled { get; set; }
}
