using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations;

public class PasswordResetRequestCF : IEntityTypeConfiguration<PasswordResetRequest>
{
    public void Configure(EntityTypeBuilder<PasswordResetRequest> builder)
    {
        builder.ToTable("PasswordResetRequests");

        builder.HasKey(x => x.PasswordResetRequestId);

        builder.Property(x => x.OtpCodeHash)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.OtpSentAtUtc)
            .IsRequired();

        builder.Property(x => x.OtpExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.SendWindowStartedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<PasswordResetRequest>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
