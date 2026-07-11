using FluentValidation;
using R_SS.BLL.DTOs.TechnicalOrder;

namespace R_SS.BLL.Validators.TechnicalOrder;

public class AssignTechnicianRequestValidator : AbstractValidator<AssignTechnicianRequest>
{
    public AssignTechnicianRequestValidator()
    {
        RuleFor(request => request.RepairOrderId).GreaterThan(0);
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.AssignedByUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
    }
}
