using R_SS.BLL.DTOs.Feedback;

namespace R_SS.Web.Models;

public class ServiceRatingViewModel
{
    public EligibleFeedbackListResponse EligibleItems { get; set; } = new();
    public SubmitFeedbackRequest Feedback { get; set; } = new();
}
