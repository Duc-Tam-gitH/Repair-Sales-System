using FluentValidation;
using R_SS.BLL.DTOs.Authentication;

namespace R_SS.BLL.Validators.Authentication;

public class UpdatePersonalInfoRequestValidator : AbstractValidator<UpdatePersonalInfoRequest>
{
    public UpdatePersonalInfoRequestValidator()
    {
        RuleFor(request => request.UserId).GreaterThan(0);
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(request => request.Phone)
            .Matches(@"^\+?[0-9]{9,15}$")
            .When(request => !string.IsNullOrWhiteSpace(request.Phone));
        RuleFor(request => request.Address).MaximumLength(255);
    }
}
