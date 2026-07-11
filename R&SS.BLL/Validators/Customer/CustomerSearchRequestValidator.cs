using FluentValidation;
using R_SS.BLL.DTOs.Customer;

namespace R_SS.BLL.Validators.Customer;

public class CustomerSearchRequestValidator : AbstractValidator<CustomerSearchRequest>
{
    public CustomerSearchRequestValidator()
    {
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.Keyword).MaximumLength(100);
    }
}
