using FluentValidation;
using R_SS.BLL.DTOs.Account;

namespace R_SS.BLL.Validators.Account;

public class ManageAccountRequestValidator : AbstractValidator<ManageAccountRequest>
{
    public ManageAccountRequestValidator()
    {
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.Username).NotEmpty().MinimumLength(4).MaximumLength(50);
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(request => request.Phone).Matches(@"^\+?[0-9]{9,15}$").When(request => !string.IsNullOrWhiteSpace(request.Phone));
        RuleFor(request => request.RoleName).NotEmpty();
    }
}
