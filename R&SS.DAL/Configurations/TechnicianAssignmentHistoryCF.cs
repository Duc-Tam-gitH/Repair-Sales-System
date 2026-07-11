using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class TechnicianAssignmentHistoryCF : IEntityTypeConfiguration<TechnicianAssignmentHistory>
    {
        public void Configure(EntityTypeBuilder<TechnicianAssignmentHistory> builder)
        {
            builder.ToTable("TechnicianAssignmentHistories");
            builder.HasKey(history => history.TechnicianAssignmentHistoryId);
            builder.Property(history => history.Notes).HasMaxLength(255);
            builder.HasOne(history => history.RepairOrder)
                .WithMany(order => order.AssignmentHistories)
                .HasForeignKey(history => history.RepairOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
