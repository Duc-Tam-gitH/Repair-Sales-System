namespace R_SS.BLL.DTOs.Invoice;

public class PrintInvoiceRequest
{
    public int TransactionId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public bool ExportPdf { get; set; }
    public bool SendEmail { get; set; }
}
