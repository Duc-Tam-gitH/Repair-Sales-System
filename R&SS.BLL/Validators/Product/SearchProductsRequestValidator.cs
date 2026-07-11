using FluentValidation;
using R_SS.BLL.DTOs.Product;

namespace R_SS.BLL.Validators.Product;

public class SearchProductsRequestValidator : AbstractValidator<SearchProductsRequest>
{
    private static readonly string[] ValidCriteria = ["all", "name", "category", "brand"];

    public SearchProductsRequestValidator()
    {
        RuleFor(request => request.Keyword).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Criteria)
            .NotEmpty()
            .Must(criteria => ValidCriteria.Contains(criteria.Trim().ToLower()))
            .WithMessage("Criteria must be all, name, category, or brand.");
    }
}
