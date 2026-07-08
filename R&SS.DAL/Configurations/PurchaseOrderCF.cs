using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class PurchaseOrderCF : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder.ToTable("PurchaseOrders");

            builder.HasKey(x => x.PurchaseOrderId);

            builder.Property(x => x.PurchaseOrderCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.Notes)
                .HasMaxLength(255);

            builder.Property(x => x.OrderDate)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasIndex(x => x.PurchaseOrderCode).IsUnique();

            builder.HasOne(x => x.Supplier)
                .WithMany(x => x.PurchaseOrders)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
