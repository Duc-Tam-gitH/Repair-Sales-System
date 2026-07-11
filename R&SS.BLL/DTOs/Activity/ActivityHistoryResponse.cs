namespace R_SS.BLL.DTOs.Activity;

public class ActivityHistoryResponse
{
    public IReadOnlyCollection<ActivityItemResponse> Activities { get; set; } = Array.Empty<ActivityItemResponse>();
    public string Message { get; set; } = string.Empty;
}
