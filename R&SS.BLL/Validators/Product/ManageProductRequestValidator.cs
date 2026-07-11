using FluentValidation;
using R_SS.BLL.DTOs.Product;

namespace R_SS.BLL.Validators.Product;

public class ManageProductRequestValidator : AbstractValidator<ManageProductRequest>
{
    public ManageProductRequestValidator()
    {
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(request => request.ProductName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.ProductCategoryId).GreaterThan(0);
        RuleFor(request => request.SalePrice).GreaterThanOrEqualTo(0);
        RuleFor(request => request.QuantityInStock).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Description).MaximumLength(255);
        RuleFor(request => request.ImageUrl).MaximumLength(500);
    }
}
