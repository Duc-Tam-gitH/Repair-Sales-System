using FluentValidation;
using R_SS.BLL.DTOs.Authentication;

namespace R_SS.BLL.Validators.Authentication;

public class VerifyForgotPasswordOtpRequestValidator : AbstractValidator<VerifyForgotPasswordOtpRequest>
{
    public VerifyForgotPasswordOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(100);

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("OTP code is required.")
            .Matches("^[0-9]{6}$").WithMessage("OTP code must contain exactly 6 digits.");
    }
}
