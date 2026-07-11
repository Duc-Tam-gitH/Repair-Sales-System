namespace R_SS.BLL.DTOs.Feedback;

public class EligibleFeedbackListResponse
{
    public IReadOnlyCollection<EligibleFeedbackItemResponse> Items { get; set; } = Array.Empty<EligibleFeedbackItemResponse>();
    public string Message { get; set; } = string.Empty;
}
