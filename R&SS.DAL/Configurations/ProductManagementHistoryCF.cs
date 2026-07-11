using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class ProductManagementHistoryCF : IEntityTypeConfiguration<ProductManagementHistory>
    {
        public void Configure(EntityTypeBuilder<ProductManagementHistory> builder)
        {
            builder.ToTable("ProductManagementHistories");
            builder.HasKey(history => history.ProductManagementHistoryId);
            builder.Property(history => history.Operation).HasMaxLength(30).IsRequired();
            builder.Property(history => history.ChangedContent).HasMaxLength(1000);
        }
    }
}
