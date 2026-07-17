using System.ComponentModel.DataAnnotations;

namespace R_SS.BLL.DTOs.Authentication;

public class SmtpDiagnosticRequest
{
    [Required]
    [EmailAddress]
    public string RecipientEmail { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = "SMTP Diagnostic Test";

    [Required]
    public string Body { get; set; } = "This is a temporary SMTP diagnostic email from Repair & Sales System.";
}
