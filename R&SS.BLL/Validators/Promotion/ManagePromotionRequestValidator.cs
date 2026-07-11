using FluentValidation;
using R_SS.BLL.DTOs.Promotion;

namespace R_SS.BLL.Validators.Promotion;

public class ManagePromotionRequestValidator : AbstractValidator<ManagePromotionRequest>
{
    public ManagePromotionRequestValidator()
    {
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.ProgramName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.PromotionType)
            .NotEmpty()
            .Must(value => value.Equals("Percentage", StringComparison.OrdinalIgnoreCase) || value.Equals("FixedAmount", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Promotion type must be Percentage or FixedAmount.");
        RuleFor(request => request.PromotionValue).GreaterThan(0);
        RuleFor(request => request.PromotionValue)
            .LessThanOrEqualTo(100)
            .When(request => request.PromotionType.Equals("Percentage", StringComparison.OrdinalIgnoreCase));
        RuleFor(request => request.EndDateUtc)
            .GreaterThanOrEqualTo(request => request.StartDateUtc)
            .WithMessage("End date must be greater than or equal to start date.");
    }
}
