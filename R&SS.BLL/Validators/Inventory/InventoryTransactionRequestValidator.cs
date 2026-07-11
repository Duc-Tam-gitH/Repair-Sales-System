using FluentValidation;
using R_SS.BLL.DTOs.Inventory;

namespace R_SS.BLL.Validators.Inventory;

public class InventoryTransactionRequestValidator : AbstractValidator<InventoryTransactionRequest>
{
    private static readonly string[] ValidTypes = ["Receipt", "Issue", "Adjustment"];

    public InventoryTransactionRequestValidator()
    {
        RuleFor(request => request.ProductId).GreaterThan(0);
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.TransactionType).Must(value => ValidTypes.Contains(value)).WithMessage("Transaction type must be Receipt, Issue, or Adjustment.");
        RuleFor(request => request.Quantity).GreaterThan(0);
        RuleFor(request => request.Reason).MaximumLength(255);
    }
}
