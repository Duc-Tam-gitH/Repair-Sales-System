using FluentValidation;
using R_SS.BLL.DTOs.Authentication;

namespace R_SS.BLL.Validators.Authentication;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty().WithMessage("Email or username is required.")
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8)
            .MaximumLength(100);
    }
}
