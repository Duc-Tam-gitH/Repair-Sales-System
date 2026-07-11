using MailKit.Net.Smtp;
using MimeKit;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendPasswordResetOtpAsync(string email, string fullName, string otpCode)
    {
        await SendPlainTextAsync(
            email,
            fullName,
            "Password Reset OTP",
            $"Hello {fullName},\n\nYour password reset OTP is: {otpCode}\nThis code will expire in 15 minutes.\n\nIf you did not request this reset, please ignore this email.");
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

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, useSsl);

        if (!string.IsNullOrWhiteSpace(username))
        {
            await client.AuthenticateAsync(username, password ?? string.Empty);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
