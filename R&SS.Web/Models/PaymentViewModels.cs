using R_SS.BLL.DTOs.Invoice;
using R_SS.Models.Entities;

namespace R_SS.Web.Models;

public class PaymentListViewModel
{
    public IReadOnlyCollection<Payment> Payments { get; set; } = Array.Empty<Payment>();
    public string? Keyword { get; set; }
    public string? Status { get; set; }
}

public class PaymentInvoiceViewModel
{
    public Payment? Payment { get; set; }
    public InvoiceResponse? Invoice { get; set; }
}

public class RepairPaymentViewModel
{
    public IReadOnlyCollection<RepairOrder> CompletedRepairs { get; set; } = Array.Empty<RepairOrder>();
}
