using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class ProductCategoryCF : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.ToTable("ProductCategories");

            builder.HasKey(x => x.ProductCategoryId);

            builder.Property(x => x.CategoryName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(255);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasIndex(x => x.CategoryName).IsUnique();
        }
    }
}
