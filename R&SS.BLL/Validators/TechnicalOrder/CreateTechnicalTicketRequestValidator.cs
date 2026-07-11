using FluentValidation;
using R_SS.BLL.DTOs.TechnicalOrder;

namespace R_SS.BLL.Validators.TechnicalOrder;

public class CreateTechnicalTicketRequestValidator : AbstractValidator<CreateTechnicalTicketRequest>
{
    public CreateTechnicalTicketRequestValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.ReceivedByUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(request => request.CustomerPhone).NotEmpty().Matches(@"^\+?[0-9]{9,15}$");
        RuleFor(request => request.DeviceType).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Brand).NotEmpty().MaximumLength(100);
        RuleFor(request => request.DeviceModel).MaximumLength(150);
        RuleFor(request => request.RequestType)
            .NotEmpty()
            .Must(value => value.Equals("Warranty", StringComparison.OrdinalIgnoreCase) || value.Equals("Repair", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Request type must be Warranty or Repair.");
        RuleFor(request => request.IssueDescription).NotEmpty().MaximumLength(255);
        RuleFor(request => request.DeviceCondition).NotEmpty().MaximumLength(255);
        RuleFor(request => request.Notes).MaximumLength(255);
    }
}
