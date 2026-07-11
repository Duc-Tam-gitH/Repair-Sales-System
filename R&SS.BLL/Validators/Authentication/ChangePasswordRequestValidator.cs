using FluentValidation;
using R_SS.BLL.DTOs.Authentication;

namespace R_SS.BLL.Validators.Authentication;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.UserId).GreaterThan(0);
        RuleFor(request => request.CurrentPassword).NotEmpty();
        RuleFor(request => request.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("New password must contain at least one number.");
        RuleFor(request => request.ConfirmNewPassword)
            .Equal(request => request.NewPassword)
            .WithMessage("Confirm new password must match new password.");
    }
}
