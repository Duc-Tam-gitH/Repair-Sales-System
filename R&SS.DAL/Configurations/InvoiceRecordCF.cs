using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class InvoiceRecordCF : IEntityTypeConfiguration<InvoiceRecord>
    {
        public void Configure(EntityTypeBuilder<InvoiceRecord> builder)
        {
            builder.ToTable("InvoiceRecords");
            builder.HasKey(x => x.InvoiceRecordId);
            builder.Property(x => x.InvoiceCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.InvoiceType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.PdfPath).HasMaxLength(500);
            builder.Property(x => x.SentToEmail).HasMaxLength(100);
            builder.HasIndex(x => x.InvoiceCode).IsUnique();
        }
    }
}
