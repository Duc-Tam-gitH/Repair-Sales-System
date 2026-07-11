using FluentValidation;
using R_SS.BLL.DTOs.RolePermission;

namespace R_SS.BLL.Validators.RolePermission;

public class DeleteRoleRequestValidator : AbstractValidator<DeleteRoleRequest>
{
    public DeleteRoleRequestValidator()
    {
        RuleFor(request => request.RoleId).GreaterThan(0);
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
    }
}
