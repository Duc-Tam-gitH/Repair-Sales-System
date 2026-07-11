using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class PromotionProductCF : IEntityTypeConfiguration<PromotionProduct>
    {
        public void Configure(EntityTypeBuilder<PromotionProduct> builder)
        {
            builder.ToTable("PromotionProducts");
            builder.HasKey(item => item.PromotionProductId);
            builder.HasIndex(item => new { item.PromotionId, item.ProductId }).IsUnique();
        }
    }
}
