using FluentValidation;
using R_SS.BLL.DTOs.Configuration;

namespace R_SS.BLL.Validators.Configuration;

public class UpdateSystemConfigurationRequestValidator : AbstractValidator<UpdateSystemConfigurationRequest>
{
    public UpdateSystemConfigurationRequestValidator()
    {
        RuleFor(request => request.Key).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Value).NotEmpty().MaximumLength(500);
        RuleFor(request => request.GroupName).MaximumLength(100);
        RuleFor(request => request.Description).MaximumLength(255);
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
    }
}
