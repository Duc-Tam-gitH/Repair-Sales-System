using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class SupplierCF : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Suppliers");

            builder.HasKey(x => x.SupplierId);

            builder.Property(x => x.SupplierCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.SupplierName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.ContactName)
                .HasMaxLength(100);

            builder.Property(x => x.Phone)
                .HasMaxLength(20);

            builder.Property(x => x.Email)
                .HasMaxLength(100);

            builder.Property(x => x.Address)
                .HasMaxLength(255);

            builder.Property(x => x.TaxCode)
                .HasMaxLength(50);

            builder.Property(x => x.Notes)
                .HasMaxLength(255);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasIndex(x => x.SupplierCode).IsUnique();
        }
    }
}
