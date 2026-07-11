using FluentValidation;
using R_SS.BLL.DTOs.Notification;

namespace R_SS.BLL.Validators.Notification;

public class UpdateNotificationTemplateRequestValidator : AbstractValidator<UpdateNotificationTemplateRequest>
{
    public UpdateNotificationTemplateRequestValidator()
    {
        RuleFor(request => request.TemplateCode).NotEmpty().MaximumLength(50);
        RuleFor(request => request.Subject).MaximumLength(255);
        RuleFor(request => request.Content).NotEmpty().MaximumLength(4000);
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
    }
}
