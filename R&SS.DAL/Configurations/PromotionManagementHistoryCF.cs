using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class PromotionManagementHistoryCF : IEntityTypeConfiguration<PromotionManagementHistory>
    {
        public void Configure(EntityTypeBuilder<PromotionManagementHistory> builder)
        {
            builder.ToTable("PromotionManagementHistories");
            builder.HasKey(history => history.PromotionManagementHistoryId);
            builder.Property(history => history.Operation).HasMaxLength(30).IsRequired();
            builder.Property(history => history.ChangedContent).HasMaxLength(1000);
        }
    }
}
