using FluentValidation;
using R_SS.BLL.DTOs.RolePermission;

namespace R_SS.BLL.Validators.RolePermission;

public class ManageRoleRequestValidator : AbstractValidator<ManageRoleRequest>
{
    public ManageRoleRequestValidator()
    {
        RuleFor(request => request.RoleName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Description).MaximumLength(255);
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
    }
}
