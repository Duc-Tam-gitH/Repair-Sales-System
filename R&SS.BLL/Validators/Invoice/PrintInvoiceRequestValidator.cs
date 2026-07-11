using FluentValidation;
using R_SS.BLL.DTOs.Invoice;

namespace R_SS.BLL.Validators.Invoice;

public class PrintInvoiceRequestValidator : AbstractValidator<PrintInvoiceRequest>
{
    public PrintInvoiceRequestValidator()
    {
        RuleFor(request => request.TransactionId).GreaterThan(0);
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.TransactionType)
            .Must(value => value.Equals("Sales", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Repair", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Transaction type must be Sales or Repair.");
    }
}
