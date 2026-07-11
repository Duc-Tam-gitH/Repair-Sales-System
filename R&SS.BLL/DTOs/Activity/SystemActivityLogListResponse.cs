namespace R_SS.BLL.DTOs.Activity;

public class SystemActivityLogListResponse
{
    public IReadOnlyCollection<SystemActivityLogResponse> Logs { get; set; } = Array.Empty<SystemActivityLogResponse>();
    public string Message { get; set; } = string.Empty;
}
