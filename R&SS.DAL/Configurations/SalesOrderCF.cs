using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class SalesOrderCF : IEntityTypeConfiguration<SalesOrder>
    {
        public void Configure(EntityTypeBuilder<SalesOrder> builder)
        {
            builder.ToTable("SalesOrders");

            builder.HasKey(x => x.SalesOrderId);

            builder.Property(x => x.SalesOrderCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.SubTotal)
                .HasPrecision(18, 2);

            builder.Property(x => x.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.TaxAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.Notes)
                .HasMaxLength(255);

            builder.Property(x => x.SalesDate)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasIndex(x => x.SalesOrderCode).IsUnique();

            builder.HasOne(x => x.Customer)
                .WithMany(x => x.SalesOrders)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
