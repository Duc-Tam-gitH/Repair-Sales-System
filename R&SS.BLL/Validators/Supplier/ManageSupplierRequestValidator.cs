using FluentValidation;
using R_SS.BLL.DTOs.Supplier;

namespace R_SS.BLL.Validators.Supplier;

public class ManageSupplierRequestValidator : AbstractValidator<ManageSupplierRequest>
{
    public ManageSupplierRequestValidator()
    {
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.SupplierCode).NotEmpty().MaximumLength(50);
        RuleFor(request => request.SupplierName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.ContactName).MaximumLength(100);
        RuleFor(request => request.Phone).MaximumLength(20);
        RuleFor(request => request.Email).MaximumLength(100).EmailAddress().When(request => !string.IsNullOrWhiteSpace(request.Email));
        RuleFor(request => request.Address).MaximumLength(255);
        RuleFor(request => request.TaxCode).MaximumLength(50);
        RuleFor(request => request.Notes).MaximumLength(255);
    }
}
