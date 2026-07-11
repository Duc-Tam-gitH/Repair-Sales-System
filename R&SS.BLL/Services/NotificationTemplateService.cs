using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Notification;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public partial class NotificationTemplateService : INotificationTemplateService
{
    private static readonly IReadOnlyCollection<NotificationTemplate> DefaultTemplates =
    [
        BuildDefault("SEND_OTP", "Email", "Send OTP", "Your OTP Code", "Hello {full_name}, your OTP code is {otp_code}.", "{full_name},{otp_code},{creation_date}"),
        BuildDefault("NEW_ORDER", "Email", "New Order Notification", "New Order {order_code}", "Hello {full_name}, your order {order_code} was created on {creation_date}. Total: {total_amount}.", "{full_name},{order_code},{creation_date},{total_amount}"),
        BuildDefault("ORDER_STATUS_UPDATE", "Email", "Order Status Update Notification", "Order {order_code} Updated", "Hello {full_name}, your order {order_code} status was updated.", "{full_name},{order_code},{creation_date},{total_amount}"),
        BuildDefault("DELIVERY_CONFIRMATION", "Email", "Delivery Confirmation", "Ticket {ticket_code} Delivery", "Hello {full_name}, ticket {ticket_code} is ready. OTP: {otp_code}.", "{full_name},{ticket_code},{otp_code},{creation_date}"),
        BuildDefault("PAYMENT_SUCCESSFUL", "Email", "Payment Successful Notification", "Payment Successful", "Hello {full_name}, payment for {order_code} was successful. Total: {total_amount}.", "{full_name},{order_code},{ticket_code},{total_amount},{creation_date}")
    ];

    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateNotificationTemplateRequest> _validator;

    public NotificationTemplateService(IUnitOfWork unitOfWork, IValidator<UpdateNotificationTemplateRequest> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<IReadOnlyCollection<NotificationTemplateResponse>> GetTemplatesAsync(string actorRole)
    {
        EnsureAdmin(actorRole);
        await EnsureDefaultsAsync();
        var templates = await _unitOfWork.NotificationTemplates.GetWithDefaultsAsync();
        return templates.Select(template => Map(template, string.Empty)).ToArray();
    }

    public async Task<NotificationTemplateResponse> UpdateAsync(UpdateNotificationTemplateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _validator.ValidateAsync(request));
        EnsureAdmin(request.ActorRole);
        await EnsureDefaultsAsync();

        var template = await _unitOfWork.NotificationTemplates.GetByCodeAsync(request.TemplateCode.Trim())
            ?? throw new NotFoundException("Notification template not found.");
        ValidateVariables(template, request.Subject, request.Content);
        await SaveHistoryAsync(template, request.ActorUserId, request.Subject, request.Content, "Update");
        template.Subject = Normalize(request.Subject);
        template.Content = request.Content.Trim();
        template.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.NotificationTemplates.Update(template);
        await LogAsync(request.ActorUserId, template.TemplateCode, "Update");
        await _unitOfWork.SaveChangesAsync();
        return Map(template, "Notification template updated successfully.");
    }

    public async Task<NotificationTemplatePreviewResponse> PreviewAsync(PreviewNotificationTemplateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureAdmin(request.ActorRole);
        await EnsureDefaultsAsync();

        var template = await _unitOfWork.NotificationTemplates.GetByCodeAsync(request.TemplateCode.Trim())
            ?? throw new NotFoundException("Notification template not found.");
        var subject = Normalize(request.Subject) ?? template.Subject;
        var content = string.IsNullOrWhiteSpace(request.Content) ? template.Content : request.Content.Trim();
        ValidateVariables(template, subject, content);

        return new NotificationTemplatePreviewResponse
        {
            TemplateCode = template.TemplateCode,
            Subject = ReplaceVariables(subject, request.SampleData),
            Content = ReplaceVariables(content, request.SampleData) ?? string.Empty,
            Message = "Notification template preview generated successfully."
        };
    }

    public async Task<NotificationTemplateResponse> RestoreDefaultAsync(RestoreNotificationTemplateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureAdmin(request.ActorRole);
        if (request.ActorUserId <= 0)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.ActorUserId), "ActorUserId must be greater than 0.") });
        }

        await EnsureDefaultsAsync();
        var template = await _unitOfWork.NotificationTemplates.GetByCodeAsync(request.TemplateCode.Trim())
            ?? throw new NotFoundException("Notification template not found.");
        await SaveHistoryAsync(template, request.ActorUserId, template.DefaultSubject, template.DefaultContent, "RestoreDefault");
        template.Subject = template.DefaultSubject;
        template.Content = template.DefaultContent;
        template.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.NotificationTemplates.Update(template);
        await LogAsync(request.ActorUserId, template.TemplateCode, "Restore Default");
        await _unitOfWork.SaveChangesAsync();
        return Map(template, "Notification template restored to default successfully.");
    }

    private async Task EnsureDefaultsAsync()
    {
        foreach (var defaultTemplate in DefaultTemplates)
        {
            if (await _unitOfWork.NotificationTemplates.GetByCodeAsync(defaultTemplate.TemplateCode) is not null)
            {
                continue;
            }

            await _unitOfWork.NotificationTemplates.AddAsync(new NotificationTemplate
            {
                TemplateCode = defaultTemplate.TemplateCode,
                TemplateType = defaultTemplate.TemplateType,
                TemplateName = defaultTemplate.TemplateName,
                Subject = defaultTemplate.Subject,
                Content = defaultTemplate.Content,
                DefaultSubject = defaultTemplate.DefaultSubject,
                DefaultContent = defaultTemplate.DefaultContent,
                AllowedVariables = defaultTemplate.AllowedVariables,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task SaveHistoryAsync(NotificationTemplate template, int actorUserId, string? newSubject, string newContent, string action)
    {
        await _unitOfWork.NotificationTemplateHistories.AddAsync(new NotificationTemplateHistory
        {
            NotificationTemplateId = template.NotificationTemplateId,
            NotificationTemplate = template,
            EditedByUserId = actorUserId,
            PreviousSubject = template.Subject,
            PreviousContent = template.Content,
            NewSubject = Normalize(newSubject),
            NewContent = newContent.Trim(),
            Action = action,
            EditedAtUtc = DateTime.UtcNow
        });
    }

    private async Task LogAsync(int actorUserId, string templateCode, string action)
    {
        await _unitOfWork.SystemActivityLogs.AddAsync(new SystemActivityLog
        {
            ActorUserId = actorUserId,
            FunctionName = "Manage Notification Templates",
            OperationType = action,
            AffectedData = templateCode,
            ExecutionResult = "Success",
            ExecutedAtUtc = DateTime.UtcNow
        });
    }

    private static void ValidateVariables(NotificationTemplate template, string? subject, string content)
    {
        var allowedVariables = ParseVariables(template.AllowedVariables);
        var usedVariables = VariableRegex().Matches($"{subject} {content}")
            .Select(match => match.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var invalidVariables = usedVariables.Where(variable => !allowedVariables.Contains(variable, StringComparer.OrdinalIgnoreCase)).ToArray();
        if (invalidVariables.Length > 0)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(UpdateNotificationTemplateRequest.Content), $"Invalid variables: {string.Join(", ", invalidVariables)}.") });
        }
    }

    private static string? ReplaceVariables(string? value, IDictionary<string, string> sampleData)
    {
        if (value is null)
        {
            return null;
        }

        return VariableRegex().Replace(value, match =>
        {
            var key = match.Value.Trim('{', '}');
            return sampleData.TryGetValue(key, out var replacement) ? replacement : $"Sample {key}";
        });
    }

    private static NotificationTemplateResponse Map(NotificationTemplate template, string message) => new()
    {
        NotificationTemplateId = template.NotificationTemplateId,
        TemplateCode = template.TemplateCode,
        TemplateType = template.TemplateType,
        TemplateName = template.TemplateName,
        Subject = template.Subject,
        Content = template.Content,
        AllowedVariables = ParseVariables(template.AllowedVariables),
        UpdatedAtUtc = template.UpdatedAtUtc,
        Message = message
    };

    private static string[] ParseVariables(string value) => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static NotificationTemplate BuildDefault(string code, string type, string name, string subject, string content, string variables) => new()
    {
        TemplateCode = code,
        TemplateType = type,
        TemplateName = name,
        Subject = subject,
        Content = content,
        DefaultSubject = subject,
        DefaultContent = content,
        AllowedVariables = variables
    };

    private static void EnsureAdmin(string role)
    {
        if (!role.Equals(RoleConstants.Admin, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Administrator can manage notification templates.");
        }
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    [GeneratedRegex(@"\{[a-zA-Z0-9_]+\}")]
    private static partial Regex VariableRegex();
}
