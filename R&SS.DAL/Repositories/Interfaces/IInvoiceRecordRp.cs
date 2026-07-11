using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IInvoiceRecordRp : IGenericRp<InvoiceRecord>
    {
        Task<bool> ExistsCodeAsync(string invoiceCode);
    }
}
