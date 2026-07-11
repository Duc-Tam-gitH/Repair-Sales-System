using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class CustomerUpdateHistoryCF : IEntityTypeConfiguration<CustomerUpdateHistory>
    {
        public void Configure(EntityTypeBuilder<CustomerUpdateHistory> builder)
        {
            builder.ToTable("CustomerUpdateHistories");

            builder.HasKey(history => history.CustomerUpdateHistoryId);

            builder.Property(history => history.ChangedContent)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(history => history.UpdatedAtUtc)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(history => history.Customer)
                .WithMany(customer => customer.UpdateHistories)
                .HasForeignKey(history => history.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(history => history.CustomerId);
            builder.HasIndex(history => history.UpdatedByUserId);
        }
    }
}
