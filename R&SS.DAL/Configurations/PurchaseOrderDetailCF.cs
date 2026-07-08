using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class PurchaseOrderDetailCF : IEntityTypeConfiguration<PurchaseOrderDetail>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderDetail> builder)
        {
            builder.ToTable("PurchaseOrderDetails");

            builder.HasKey(x => x.PurchaseOrderDetailId);

            builder.Property(x => x.UnitCost)
                .HasPrecision(18, 2);

            builder.Property(x => x.LineTotal)
                .HasPrecision(18, 2);

            builder.HasOne(x => x.PurchaseOrder)
                .WithMany(x => x.PurchaseOrderDetails)
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.PurchaseOrderDetails)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
