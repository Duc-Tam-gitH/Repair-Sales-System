using FluentValidation;
using R_SS.BLL.DTOs.Feedback;

namespace R_SS.BLL.Validators.Feedback;

public class SubmitFeedbackRequestValidator : AbstractValidator<SubmitFeedbackRequest>
{
    public SubmitFeedbackRequestValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.Type)
            .NotEmpty()
            .Must(value => value.Equals("order", StringComparison.OrdinalIgnoreCase) || value.Equals("technical", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Type must be order or technical.");
        RuleFor(request => request.ItemId).GreaterThan(0);
        RuleFor(request => request.Rating).InclusiveBetween(1, 5);
        RuleFor(request => request.Comment).MaximumLength(1000);
    }
}
