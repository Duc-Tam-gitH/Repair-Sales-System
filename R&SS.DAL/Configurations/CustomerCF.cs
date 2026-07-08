using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class CustomerCF : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");

            builder.HasKey(x => x.CustomerId);

            builder.Property(x => x.CustomerCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FullName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Phone)
                .HasMaxLength(20);

            builder.Property(x => x.Email)
                .HasMaxLength(100);

            builder.Property(x => x.Address)
                .HasMaxLength(255);

            builder.Property(x => x.Notes)
                .HasMaxLength(255);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasIndex(x => x.CustomerCode).IsUnique();
        }
    }
}
