using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class CartItemCF : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");

            builder.HasKey(item => item.CartItemId);

            builder.Property(item => item.UnitPrice)
                .HasPrecision(18, 2);

            builder.HasIndex(item => new { item.CartId, item.ProductId })
                .IsUnique();

            builder.HasOne(item => item.Cart)
                .WithMany(cart => cart.CartItems)
                .HasForeignKey(item => item.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
