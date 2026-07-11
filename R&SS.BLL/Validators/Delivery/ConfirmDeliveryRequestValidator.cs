using FluentValidation;
using R_SS.BLL.DTOs.Delivery;

namespace R_SS.BLL.Validators.Delivery;

public class ConfirmDeliveryRequestValidator : AbstractValidator<ConfirmDeliveryRequest>
{
    public ConfirmDeliveryRequestValidator()
    {
        RuleFor(request => request.RepairOrderId).GreaterThan(0);
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.OtpCode).NotEmpty().Length(6);
        RuleFor(request => request.IpAddress).MaximumLength(50);
    }
}
