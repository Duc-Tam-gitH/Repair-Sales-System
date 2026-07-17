namespace R_SS.BLL.Interfaces;

public interface IEmailSender
{
    /// <summary>
    /// Sends a password reset OTP to the specified email address.
    /// </summary>
    /// <param name="email">Recipient email address.</param>
    /// <param name="fullName">Recipient display name.</param>
    /// <param name="otpCode">The OTP code to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendPasswordResetOtpAsync(string email, string fullName, string otpCode);

    /// <summary>
    /// Sends a temporary diagnostic email to verify SMTP delivery.
    /// </summary>
    Task SendDiagnosticEmailAsync(string email, string subject, string body);

    /// <summary>
    /// Sends a technical ticket creation notification.
    /// </summary>
    Task SendTechnicalTicketCreatedAsync(string email, string fullName, string ticketCode);

    /// <summary>
    /// Sends a delivery confirmation OTP for a technical ticket.
    /// </summary>
    Task SendDeliveryConfirmationOtpAsync(string email, string fullName, string ticketCode, string otpCode);

    /// <summary>
    /// Sends a technical ticket completion notification.
    /// </summary>
    Task SendTechnicalTicketCompletedAsync(string email, string fullName, string ticketCode);

    /// <summary>
    /// Sends a customer-facing service request receipt notification.
    /// </summary>
    Task SendServiceRequestReceivedAsync(string email, string fullName, string requestCode);

    /// <summary>
    /// Sends an internal notification when a customer submits a service request.
    /// </summary>
    Task SendInternalServiceRequestNotificationAsync(string requestCode);

    /// <summary>
    /// Sends an invoice notification to the customer.
    /// </summary>
    Task SendInvoiceAsync(string email, string fullName, string invoiceCode);

    Task SendOrderCancelledAsync(string email, string fullName, string orderCode, string reason);

    Task SendTechnicalTicketCancelledAsync(string email, string fullName, string ticketCode, string reason);

    Task SendTechnicianTicketCancelledAsync(string email, string fullName, string ticketCode, string reason);
}
