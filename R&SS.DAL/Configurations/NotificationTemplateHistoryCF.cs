using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class NotificationTemplateHistoryCF : IEntityTypeConfiguration<NotificationTemplateHistory>
    {
        public void Configure(EntityTypeBuilder<NotificationTemplateHistory> builder)
        {
            builder.ToTable("NotificationTemplateHistories");
            builder.HasKey(x => x.NotificationTemplateHistoryId);
            builder.Property(x => x.PreviousSubject).HasMaxLength(255);
            builder.Property(x => x.PreviousContent).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.NewSubject).HasMaxLength(255);
            builder.Property(x => x.NewContent).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.Action).HasMaxLength(30).IsRequired();
            builder.HasOne(x => x.NotificationTemplate)
                .WithMany(x => x.Histories)
                .HasForeignKey(x => x.NotificationTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
