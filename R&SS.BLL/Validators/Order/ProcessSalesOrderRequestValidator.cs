using FluentValidation;
using R_SS.BLL.DTOs.Order;

namespace R_SS.BLL.Validators.Order;

public class ProcessSalesOrderRequestValidator : AbstractValidator<ProcessSalesOrderRequest>
{
    public ProcessSalesOrderRequestValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.ActorUserId).GreaterThan(0);
        RuleFor(request => request.ActorRole).NotEmpty();
        RuleFor(request => request.PaymentMethod).NotEmpty().MaximumLength(30);
        RuleFor(request => request.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Items).NotEmpty();
        RuleForEach(request => request.Items).ChildRules(item =>
        {
            item.RuleFor(value => value.ProductId).GreaterThan(0);
            item.RuleFor(value => value.Quantity).GreaterThan(0);
        });
    }
}
