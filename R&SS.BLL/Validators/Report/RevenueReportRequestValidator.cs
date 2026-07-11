using FluentValidation;
using R_SS.BLL.DTOs.Report;

namespace R_SS.BLL.Validators.Report;

public class RevenueReportRequestValidator : AbstractValidator<RevenueReportRequest>
{
    public RevenueReportRequestValidator()
    {
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.ToUtc).GreaterThanOrEqualTo(request => request.FromUtc);
    }
}
