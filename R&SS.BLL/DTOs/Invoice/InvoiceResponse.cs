namespace R_SS.BLL.DTOs.Invoice;

public class InvoiceResponse
{
    public int InvoiceRecordId { get; set; }
    public string InvoiceCode { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public int TransactionId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? PdfPath { get; set; }
    public DateTime PrintedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
