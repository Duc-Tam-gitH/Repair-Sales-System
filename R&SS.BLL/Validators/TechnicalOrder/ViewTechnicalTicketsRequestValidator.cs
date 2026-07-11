using FluentValidation;
using R_SS.BLL.DTOs.TechnicalOrder;

namespace R_SS.BLL.Validators.TechnicalOrder;

public class ViewTechnicalTicketsRequestValidator : AbstractValidator<ViewTechnicalTicketsRequest>
{
    public ViewTechnicalTicketsRequestValidator()
    {
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.CustomerId).GreaterThan(0).When(request => request.CustomerId.HasValue);
    }
}
