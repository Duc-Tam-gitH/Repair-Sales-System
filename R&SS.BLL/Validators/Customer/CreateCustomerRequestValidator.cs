using FluentValidation;
using R_SS.BLL.DTOs.Customer;

namespace R_SS.BLL.Validators.Customer;

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(request => request.CustomerCode).NotEmpty().MaximumLength(50);
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Phone)
            .Matches(@"^\+?[0-9]{9,15}$")
            .When(request => !string.IsNullOrWhiteSpace(request.Phone));
        RuleFor(request => request.Email)
            .EmailAddress()
            .MaximumLength(100)
            .When(request => !string.IsNullOrWhiteSpace(request.Email));
        RuleFor(request => request.Address).MaximumLength(255);
        RuleFor(request => request.Notes).MaximumLength(255);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.ActorUserId).GreaterThan(0);
    }
}
