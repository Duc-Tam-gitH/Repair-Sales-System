using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class PaymentCF : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(x => x.PaymentId);

            builder.Property(x => x.PaymentCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.PaymentMethod)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.PaymentStatus)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.ReferenceNumber)
                .HasMaxLength(100);

            builder.Property(x => x.Notes)
                .HasMaxLength(255);

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2);

            builder.Property(x => x.PaymentDate)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasIndex(x => x.PaymentCode).IsUnique();

            builder.HasOne(x => x.Customer)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SalesOrder)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.SalesOrderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.RepairOrder)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.RepairOrderId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
