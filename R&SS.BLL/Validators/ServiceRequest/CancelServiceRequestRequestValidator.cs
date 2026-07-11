using FluentValidation;
using R_SS.BLL.DTOs.ServiceRequest;

namespace R_SS.BLL.Validators.ServiceRequest;

public class CancelServiceRequestRequestValidator : AbstractValidator<CancelServiceRequestRequest>
{
    public CancelServiceRequestRequestValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.CustomerId).GreaterThan(0);
    }
}
