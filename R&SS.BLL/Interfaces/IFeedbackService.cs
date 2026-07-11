using R_SS.BLL.DTOs.Feedback;

namespace R_SS.BLL.Interfaces;

public interface IFeedbackService
{
    Task<EligibleFeedbackListResponse> GetEligibleItemsAsync(int customerId);
    Task<FeedbackResponse> SubmitAsync(SubmitFeedbackRequest request);
    Task<FeedbackStatisticsResponse> GetStatisticsAsync(string actorRole);
}
