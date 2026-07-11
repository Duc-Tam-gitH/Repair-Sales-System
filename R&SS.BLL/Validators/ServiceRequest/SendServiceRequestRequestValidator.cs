using FluentValidation;
using R_SS.BLL.DTOs.ServiceRequest;

namespace R_SS.BLL.Validators.ServiceRequest;

public class SendServiceRequestRequestValidator : AbstractValidator<SendServiceRequestRequest>
{
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];

    public SendServiceRequestRequestValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.ServiceType).NotEmpty().MaximumLength(50);
        RuleFor(request => request.DeviceType).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Brand).NotEmpty().MaximumLength(100);
        RuleFor(request => request.DeviceModel).MaximumLength(150);
        RuleFor(request => request.Description).NotEmpty().MaximumLength(1000);
        RuleFor(request => request.Images).Must(images => images.Count <= 3)
            .WithMessage("A maximum of 3 images can be uploaded.");
        RuleForEach(request => request.Images).ChildRules(image =>
        {
            image.RuleFor(item => item.FileName).NotEmpty().MaximumLength(255)
                .Must(fileName => AllowedExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant()))
                .WithMessage("Only JPG and PNG images are allowed.");
            image.RuleFor(item => item.SizeBytes).InclusiveBetween(1, MaxImageSizeBytes)
                .WithMessage("Each image must be 5 MB or smaller.");
        });
    }
}
