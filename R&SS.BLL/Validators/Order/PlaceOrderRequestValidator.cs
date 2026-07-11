using FluentValidation;
using R_SS.BLL.DTOs.Order;

namespace R_SS.BLL.Validators.Order;

public class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderRequestValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.RecipientName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.DeliveryAddress).NotEmpty().MaximumLength(255);
        RuleFor(request => request.PaymentMethod).NotEmpty().MaximumLength(30);
    }
}
