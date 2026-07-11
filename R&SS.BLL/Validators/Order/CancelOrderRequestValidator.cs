using FluentValidation;
using R_SS.BLL.DTOs.Order;

namespace R_SS.BLL.Validators.Order;

public class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderRequestValidator()
    {
        RuleFor(request => request.SalesOrderId).GreaterThan(0);
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.Reason).NotEmpty().MaximumLength(500);
    }
}
