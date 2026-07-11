using FluentValidation;
using R_SS.BLL.DTOs.Activity;

namespace R_SS.BLL.Validators.Activity;

public class SystemActivityLogSearchRequestValidator : AbstractValidator<SystemActivityLogSearchRequest>
{
    public SystemActivityLogSearchRequestValidator()
    {
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.ToUtc).GreaterThanOrEqualTo(request => request.FromUtc).When(request => request.FromUtc.HasValue && request.ToUtc.HasValue);
    }
}
