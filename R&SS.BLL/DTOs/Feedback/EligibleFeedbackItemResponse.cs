namespace R_SS.BLL.DTOs.Feedback;

public class EligibleFeedbackItemResponse
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime CompletedAtUtc { get; set; }
}
