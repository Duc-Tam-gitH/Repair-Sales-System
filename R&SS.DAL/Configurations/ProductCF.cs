using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class ProductCF : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(x => x.ProductId);

            builder.Property(x => x.ProductCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ProductName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Unit)
                .HasMaxLength(50);

            builder.Property(x => x.CostPrice)
                .HasPrecision(18, 2);

            builder.Property(x => x.SalePrice)
                .HasPrecision(18, 2);

            builder.Property(x => x.Description)
                .HasMaxLength(255);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasIndex(x => x.ProductCode).IsUnique();

            builder.HasOne(x => x.ProductCategory)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.ProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Supplier)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
