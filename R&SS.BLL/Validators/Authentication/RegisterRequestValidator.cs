using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using R_SS.BLL.DTOs.Authentication;

namespace R_SS.BLL.Validators.Authentication;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(4)
            .MaximumLength(50);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm Password is required.")
            .Equal(x => x.Password)
            .WithMessage("Confirm Password does not match.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .MaximumLength(20);

        RuleFor(x => x.Address)
            .MaximumLength(255);
    }
}
