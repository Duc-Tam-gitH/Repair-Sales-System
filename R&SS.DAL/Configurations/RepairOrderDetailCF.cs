using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class RepairOrderDetailCF : IEntityTypeConfiguration<RepairOrderDetail>
    {
        public void Configure(EntityTypeBuilder<RepairOrderDetail> builder)
        {
            builder.ToTable("RepairOrderDetails");

            builder.HasKey(x => x.RepairOrderDetailId);

            builder.Property(x => x.Description)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.UnitCost)
                .HasPrecision(18, 2);

            builder.Property(x => x.LineTotal)
                .HasPrecision(18, 2);

            builder.HasOne(x => x.RepairOrder)
                .WithMany(x => x.RepairOrderDetails)
                .HasForeignKey(x => x.RepairOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.RepairOrderDetails)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
