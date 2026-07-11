using FluentValidation;
using R_SS.BLL.DTOs.Delivery;

namespace R_SS.BLL.Validators.Delivery;

public class RejectDeliveryRequestValidator : AbstractValidator<RejectDeliveryRequest>
{
    public RejectDeliveryRequestValidator()
    {
        RuleFor(request => request.RepairOrderId).GreaterThan(0);
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.Reason).NotEmpty().MaximumLength(500);
    }
}
