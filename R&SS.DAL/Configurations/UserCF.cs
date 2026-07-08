using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class UserCF : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.UserId);

            builder.Property(x => x.Username)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.FullName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Phone)
                .HasMaxLength(20);

            builder.Property(x => x.Address)
                .HasMaxLength(255);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasIndex(x => x.Username).IsUnique();
            builder.HasIndex(x => x.Email).IsUnique();
        }
    }
}
