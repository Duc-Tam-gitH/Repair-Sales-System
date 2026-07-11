using FluentValidation;
using R_SS.BLL.DTOs.Activity;

namespace R_SS.BLL.Validators.Activity;

public class ActivityHistoryRequestValidator : AbstractValidator<ActivityHistoryRequest>
{
    private static readonly string[] ValidTypes = ["all", "order", "technical"];

    public ActivityHistoryRequestValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.Type)
            .NotEmpty()
            .Must(type => ValidTypes.Contains(type.Trim().ToLower()))
            .WithMessage("Type must be all, order, or technical.");
    }
}
