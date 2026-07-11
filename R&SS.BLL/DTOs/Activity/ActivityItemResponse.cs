namespace R_SS.BLL.DTOs.Activity;

public class ActivityItemResponse
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
