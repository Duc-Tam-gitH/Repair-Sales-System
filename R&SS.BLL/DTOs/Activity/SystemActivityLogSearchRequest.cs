namespace R_SS.BLL.DTOs.Activity;

public class SystemActivityLogSearchRequest
{
    public string ActorRole { get; set; } = string.Empty;
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int? ActorUserId { get; set; }
    public string? FunctionName { get; set; }
    public string? OperationType { get; set; }
}
