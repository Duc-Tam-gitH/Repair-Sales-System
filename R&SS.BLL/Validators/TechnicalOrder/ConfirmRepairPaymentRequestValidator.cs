using FluentValidation;
using R_SS.BLL.DTOs.TechnicalOrder;

namespace R_SS.BLL.Validators.TechnicalOrder;

public class ConfirmRepairPaymentRequestValidator : AbstractValidator<ConfirmRepairPaymentRequest>
{
    public ConfirmRepairPaymentRequestValidator()
    {
        RuleFor(request => request.RepairOrderId).GreaterThan(0);
        RuleFor(request => request.ConfirmedByUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.PaymentMethod)
            .NotEmpty()
            .Must(value => value.Equals("Cash", StringComparison.OrdinalIgnoreCase) || value.Equals("Bank Transfer", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Payment method must be Cash or Bank Transfer.");
        RuleFor(request => request.ManualDeliveryNote).MaximumLength(255);
    }
}
