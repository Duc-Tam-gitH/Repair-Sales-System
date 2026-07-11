using FluentValidation;
using R_SS.BLL.DTOs.TechnicalOrder;

namespace R_SS.BLL.Validators.TechnicalOrder;

public class CancelTechnicalTicketRequestValidator : AbstractValidator<CancelTechnicalTicketRequest>
{
    public CancelTechnicalTicketRequestValidator()
    {
        RuleFor(request => request.RepairOrderId).GreaterThan(0);
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.Reason).NotEmpty().MaximumLength(500);
    }
}
