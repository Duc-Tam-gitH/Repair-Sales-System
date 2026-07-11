namespace R_SS.BLL.DTOs.Activity;

public class SystemActivityLogResponse
{
    public int SystemActivityLogId { get; set; }
    public int? ActorUserId { get; set; }
    public string? ActorUsername { get; set; }
    public string FunctionName { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string? AffectedData { get; set; }
    public string ExecutionResult { get; set; } = string.Empty;
    public DateTime ExecutedAtUtc { get; set; }
    public string? Details { get; set; }
}
