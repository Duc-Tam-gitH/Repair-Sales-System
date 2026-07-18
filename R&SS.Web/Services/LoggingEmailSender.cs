using R_SS.BLL.Interfaces;

namespace R_SS.Web.Services;

public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetOtpAsync(string email, string fullName, string otpCode)
    {
        _logger.LogInformation("Password reset OTP for {Email} ({FullName}): {OtpCode}", email, fullName, otpCode);
        return Task.CompletedTask;
    }

    public Task SendDiagnosticEmailAsync(string email, string subject, string body)
    {
        _logger.LogInformation("Diagnostic email queued for {Email}: {Subject}", email, subject);
        return Task.CompletedTask;
    }

    public Task SendTechnicalTicketCreatedAsync(string email, string fullName, string ticketCode)
    {
        _logger.LogInformation("Technical ticket created notification queued for {Email}: {TicketCode}", email, ticketCode);
        return Task.CompletedTask;
    }

    public Task SendDeliveryConfirmationOtpAsync(string email, string fullName, string ticketCode, string otpCode)
    {
        _logger.LogInformation("Delivery confirmation OTP for {Email}, ticket {TicketCode}: {OtpCode}", email, ticketCode, otpCode);
        return Task.CompletedTask;
    }

    public Task SendTechnicalTicketCompletedAsync(string email, string fullName, string ticketCode)
    {
        _logger.LogInformation("Technical ticket completed notification queued for {Email}: {TicketCode}", email, ticketCode);
        return Task.CompletedTask;
    }

    public Task SendServiceRequestReceivedAsync(string email, string fullName, string requestCode)
    {
        _logger.LogInformation("Service request receipt queued for {Email}: {RequestCode}", email, requestCode);
        return Task.CompletedTask;
    }

    public Task SendInternalServiceRequestNotificationAsync(string requestCode)
    {
        _logger.LogInformation("Internal service request notification queued: {RequestCode}", requestCode);
        return Task.CompletedTask;
    }

    public Task SendInvoiceAsync(string email, string fullName, string invoiceCode)
    {
        _logger.LogInformation("Invoice notification queued for {Email}: {InvoiceCode}", email, invoiceCode);
        return Task.CompletedTask;
    }

    public Task SendOrderCancelledAsync(string email, string fullName, string orderCode, string reason)
    {
        _logger.LogInformation("Order cancellation notification queued for {Email}: {OrderCode}", email, orderCode);
        return Task.CompletedTask;
    }

    public Task SendTechnicalTicketCancelledAsync(string email, string fullName, string ticketCode, string reason)
    {
        _logger.LogInformation("Technical ticket cancellation notification queued for {Email}: {TicketCode}", email, ticketCode);
        return Task.CompletedTask;
    }

    public Task SendTechnicianTicketCancelledAsync(string email, string fullName, string ticketCode, string reason)
    {
        _logger.LogInformation("Technician ticket cancellation notification queued for {Email}: {TicketCode}", email, ticketCode);
        return Task.CompletedTask;
    }
}
