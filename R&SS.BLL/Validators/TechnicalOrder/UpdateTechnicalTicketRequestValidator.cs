using FluentValidation;
using R_SS.BLL.DTOs.TechnicalOrder;

namespace R_SS.BLL.Validators.TechnicalOrder;

public class UpdateTechnicalTicketRequestValidator : AbstractValidator<UpdateTechnicalTicketRequest>
{
    public UpdateTechnicalTicketRequestValidator()
    {
        RuleFor(request => request.RepairOrderId).GreaterThan(0);
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.CustomerPhone).Matches(@"^\+?[0-9]{9,15}$").When(request => !string.IsNullOrWhiteSpace(request.CustomerPhone));
        RuleFor(request => request.CustomerFullName).MaximumLength(100);
        RuleFor(request => request.CustomerAddress).MaximumLength(255);
        RuleFor(request => request.InspectionResult).MaximumLength(255);
        RuleFor(request => request.Diagnosis).MaximumLength(255);
        RuleFor(request => request.DeviceCondition).MaximumLength(255);
        RuleFor(request => request.WorkPerformed).MaximumLength(255);
        RuleFor(request => request.RepairResult).MaximumLength(255);
        RuleFor(request => request.AccompanyingAccessories).MaximumLength(255);
        RuleFor(request => request.ProcessingMinutes).GreaterThanOrEqualTo(0).When(request => request.ProcessingMinutes.HasValue);
        RuleFor(request => request.ServiceFee).GreaterThanOrEqualTo(0).When(request => request.ServiceFee.HasValue);
        RuleForEach(request => request.UsedComponents).ChildRules(component =>
        {
            component.RuleFor(item => item.ProductId).GreaterThan(0);
            component.RuleFor(item => item.Quantity).GreaterThan(0);
            component.RuleFor(item => item.Notes).MaximumLength(255);
        });
    }
}
