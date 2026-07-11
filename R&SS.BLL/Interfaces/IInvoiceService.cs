using R_SS.BLL.DTOs.Invoice;

namespace R_SS.BLL.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceResponse> PrintAsync(PrintInvoiceRequest request);
}
