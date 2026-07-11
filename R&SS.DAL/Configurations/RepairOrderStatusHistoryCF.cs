using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class RepairOrderStatusHistoryCF : IEntityTypeConfiguration<RepairOrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<RepairOrderStatusHistory> builder)
        {
            builder.ToTable("RepairOrderStatusHistories");
            builder.HasKey(history => history.RepairOrderStatusHistoryId);
            builder.Property(history => history.Status).HasMaxLength(30).IsRequired();
            builder.Property(history => history.Notes).HasMaxLength(255);
            builder.HasOne(history => history.RepairOrder)
                .WithMany(order => order.StatusHistories)
                .HasForeignKey(history => history.RepairOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
