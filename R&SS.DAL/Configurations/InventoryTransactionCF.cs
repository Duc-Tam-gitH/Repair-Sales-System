using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class InventoryTransactionCF : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.ToTable("InventoryTransactions");
            builder.HasKey(transaction => transaction.InventoryTransactionId);
            builder.Property(transaction => transaction.TransactionType).HasMaxLength(30).IsRequired();
            builder.Property(transaction => transaction.Reason).HasMaxLength(255);
            builder.HasOne(transaction => transaction.Product)
                .WithMany()
                .HasForeignKey(transaction => transaction.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
