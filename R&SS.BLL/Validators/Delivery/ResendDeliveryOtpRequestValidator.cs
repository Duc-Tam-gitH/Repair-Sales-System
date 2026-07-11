using FluentValidation;
using R_SS.BLL.DTOs.Delivery;

namespace R_SS.BLL.Validators.Delivery;

public class ResendDeliveryOtpRequestValidator : AbstractValidator<ResendDeliveryOtpRequest>
{
    public ResendDeliveryOtpRequestValidator()
    {
        RuleFor(request => request.RepairOrderId).GreaterThan(0);
        RuleFor(request => request.CustomerId).GreaterThan(0);
    }
}
