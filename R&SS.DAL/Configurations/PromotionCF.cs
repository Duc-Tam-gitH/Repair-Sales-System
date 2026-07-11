using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class PromotionCF : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.ToTable("Promotions");
            builder.HasKey(promotion => promotion.PromotionId);
            builder.Property(promotion => promotion.PromotionCode).HasMaxLength(50).IsRequired();
            builder.Property(promotion => promotion.ProgramName).HasMaxLength(150).IsRequired();
            builder.Property(promotion => promotion.PromotionType).HasMaxLength(30).IsRequired();
            builder.Property(promotion => promotion.PromotionValue).HasPrecision(18, 2);
            builder.HasIndex(promotion => promotion.PromotionCode).IsUnique();
        }
    }
}
