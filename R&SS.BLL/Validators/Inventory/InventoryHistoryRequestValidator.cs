using FluentValidation;
using R_SS.BLL.DTOs.Inventory;

namespace R_SS.BLL.Validators.Inventory;

public class InventoryHistoryRequestValidator : AbstractValidator<InventoryHistoryRequest>
{
    public InventoryHistoryRequestValidator()
    {
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.ToUtc).GreaterThanOrEqualTo(request => request.FromUtc).When(request => request.FromUtc.HasValue && request.ToUtc.HasValue);
    }
}
