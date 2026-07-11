namespace R_SS.BLL.DTOs.Feedback;

public class FeedbackStatisticsResponse
{
    public int TotalFeedbackCount { get; set; }
    public double AverageRating { get; set; }
    public string Message { get; set; } = string.Empty;
}
