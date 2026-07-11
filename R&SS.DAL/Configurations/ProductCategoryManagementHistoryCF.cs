using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class ProductCategoryManagementHistoryCF : IEntityTypeConfiguration<ProductCategoryManagementHistory>
    {
        public void Configure(EntityTypeBuilder<ProductCategoryManagementHistory> builder)
        {
            builder.ToTable("ProductCategoryManagementHistories");
            builder.HasKey(history => history.ProductCategoryManagementHistoryId);
            builder.Property(history => history.Operation).HasMaxLength(30).IsRequired();
            builder.Property(history => history.ChangedContent).HasMaxLength(1000);
        }
    }
}
