using FluentValidation;
using R_SS.BLL.DTOs.ProductCategory;

namespace R_SS.BLL.Validators.ProductCategory;

public class ManageProductCategoryRequestValidator : AbstractValidator<ManageProductCategoryRequest>
{
    public ManageProductCategoryRequestValidator()
    {
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.CategoryName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Description).MaximumLength(255);
    }
}
