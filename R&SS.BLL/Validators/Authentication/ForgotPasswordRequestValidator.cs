using FluentValidation;
using R_SS.BLL.DTOs.Authentication;

namespace R_SS.BLL.Validators.Authentication;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(100);
    }
}
