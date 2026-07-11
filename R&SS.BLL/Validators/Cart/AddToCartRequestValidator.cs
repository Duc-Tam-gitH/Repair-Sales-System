using FluentValidation;
using R_SS.BLL.DTOs.Cart;

namespace R_SS.BLL.Validators.Cart;

public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartRequestValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.ProductId).GreaterThan(0);
        RuleFor(request => request.Quantity).GreaterThan(0);
    }
}
