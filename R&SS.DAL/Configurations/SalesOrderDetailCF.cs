using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class SalesOrderDetailCF : IEntityTypeConfiguration<SalesOrderDetail>
    {
        public void Configure(EntityTypeBuilder<SalesOrderDetail> builder)
        {
            builder.ToTable("SalesOrderDetails");

            builder.HasKey(x => x.SalesOrderDetailId);

            builder.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            builder.Property(x => x.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.LineTotal)
                .HasPrecision(18, 2);

            builder.HasOne(x => x.SalesOrder)
                .WithMany(x => x.SalesOrderDetails)
                .HasForeignKey(x => x.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.SalesOrderDetails)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
