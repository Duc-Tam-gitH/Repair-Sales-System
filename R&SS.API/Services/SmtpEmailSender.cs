using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetOtpAsync(string email, string fullName, string otpCode)
    {
        await SendPlainTextAsync(
            email,
            fullName,
            "Password Reset OTP",
            $"Hello {fullName},\n\nYour password reset OTP is: {otpCode}\nThis code will expire in 15 minutes.\n\nIf you did not request this reset, please ignore this email.");
    }

    public async Task SendDiagnosticEmailAsync(string email, string subject, string body)
    {
        await SendPlainTextAsync(email, email, subject, body);
    }

    public async Task SendTechnicalTicketCreatedAsync(string email, string fullName, string ticketCode)
    {
        await SendPlainTextAsync(
            email,
            fullName,
            "Technical Ticket Created",
            $"Hello {fullName},\n\nYour technical ticket {ticketCode} has been created. Please use this code to track processing progress.");
    }

    public async Task SendDeliveryConfirmationOtpAsync(string email, string fullName, string ticketCode, string otpCode)
    {
        await SendPlainTextAsync(
            email,
            fullName,
            "Delivery Confirmation OTP",
            $"Hello {fullName},\n\nYour technical ticket {ticketCode} is ready for delivery. Your confirmation OTP is: {otpCode}");
    }

    public async Task SendTechnicalTicketCompletedAsync(string email, string fullName, string ticketCode)
    {
        await SendPlainTextAsync(
            email,
            fullName,
            "Technical Ticket Completed",
            $"Hello {fullName},\n\nYour technical ticket {ticketCode} has been completed. Thank you for using our service.");
    }

    public async Task SendServiceRequestReceivedAsync(string email, string fullName, string requestCode)
    {
        await SendPlainTextAsync(
            email,
            fullName,
            "Service Request Received",
            $"Hello {fullName},\n\nYour service request {requestCode} has been received and is pending reception review.");
    }

    public async Task SendInternalServiceRequestNotificationAsync(string requestCode)
    {
        var internalEmail = _configuration["Smtp:InternalNotificationEmail"];
        if (string.IsNullOrWhiteSpace(internalEmail))
        {
            throw new InvalidOperationException("Internal notification email is missing.");
        }

        await SendPlainTextAsync(
            internalEmail,
            "Reception Team",
            "New Service Request",
            $"A new service request {requestCode} has been submitted and requires reception review.");
    }

    public async Task SendInvoiceAsync(string email, string fullName, string invoiceCode)
    {
        await SendPlainTextAsync(
            email,
            fullName,
            "Invoice",
            $"Hello {fullName},\n\nInvoice {invoiceCode} has been generated for your transaction.");
    }

    public async Task SendOrderCancelledAsync(string email, string fullName, string orderCode, string reason)
    {
        await SendPlainTextAsync(
            email,
            fullName,
            "Order Cancelled",
            $"Hello {fullName},\n\nYour order {orderCode} has been cancelled.\nReason: {reason}");
    }

    public async Task SendTechnicalTicketCancelledAsync(string email, string fullName, string ticketCode, string reason)
    {
        await SendPlainTextAsync(
            email,
            fullName,
            "Technical Ticket Cancelled",
            $"Hello {fullName},\n\nYour technical ticket {ticketCode} has been cancelled.\nReason: {reason}");
    }

    public async Task SendTechnicianTicketCancelledAsync(string email, string fullName, string ticketCode, string reason)
    {
        await SendPlainTextAsync(
            email,
            fullName,
            "Assigned Ticket Cancelled",
            $"Hello {fullName},\n\nAssigned ticket {ticketCode} has been cancelled.\nReason: {reason}");
    }

    private async Task SendPlainTextAsync(string email, string fullName, string subject, string body)
    {
        var host = _configuration["Smtp:Host"];
        var portValue = _configuration["Smtp:Port"];
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var fromEmail = _configuration["Smtp:FromEmail"];
        var fromName = _configuration["Smtp:FromName"] ?? "Repair & Sales System";
        var useSslValue = _configuration["Smtp:UseSsl"];

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(portValue) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException("SMTP configuration is missing.");
        }

        if (!int.TryParse(portValue, out var port))
        {
            throw new InvalidOperationException("SMTP port is invalid.");
        }

        var useSsl = bool.TryParse(useSslValue, out var parsedUseSsl) && parsedUseSsl;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(fullName, email));
        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = body
        };

        try
        {
            using var client = new SmtpClient();
            var socketOptions = port == 465
                ? SecureSocketOptions.SslOnConnect
                : useSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

            _logger.LogInformation(
                "Connecting to SMTP host {Host}:{Port} with {SocketOptions} for recipient {Recipient}.",
                host,
                port,
                socketOptions,
                email);

            await client.ConnectAsync(host, port, socketOptions);

            if (!string.IsNullOrWhiteSpace(username))
            {
                await client.AuthenticateAsync(username, password ?? string.Empty);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation(
                "SMTP message sent successfully to {Recipient} with subject {Subject}.",
                email,
                subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send SMTP message to {Recipient} with subject {Subject}.",
                email,
                subject);
            throw;
        }
    }
}
