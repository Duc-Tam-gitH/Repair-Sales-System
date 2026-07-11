using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class CartCF : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");

            builder.HasKey(cart => cart.CartId);

            builder.HasIndex(cart => cart.CustomerId)
                .IsUnique();

            builder.HasOne(cart => cart.Customer)
                .WithMany()
                .HasForeignKey(cart => cart.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
