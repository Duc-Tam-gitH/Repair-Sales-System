namespace R_SS.BLL.DTOs.Feedback;

public class SubmitFeedbackRequest
{
    public int CustomerId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
