using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class InvoiceRecordRp : GenericRp<InvoiceRecord>, IInvoiceRecordRp
    {
        public InvoiceRecordRp(AppDbContext context) : base(context) { }
        public async Task<bool> ExistsCodeAsync(string invoiceCode) => await _context.InvoiceRecords.AnyAsync(x => x.InvoiceCode == invoiceCode);
    }
}
