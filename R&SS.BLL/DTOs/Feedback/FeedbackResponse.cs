namespace R_SS.BLL.DTOs.Feedback;

public class FeedbackResponse
{
    public int FeedbackId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string Message { get; set; } = string.Empty;
}
