using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class InventoryTransaction
    {
        [Key]
        public int InventoryTransactionId { get; set; }
        public int ProductId { get; set; }
        public int ActorUserId { get; set; }
        [Required, MaxLength(30)]
        public string TransactionType { get; set; } = string.Empty;
        public int QuantityChange { get; set; }
        public int StockBefore { get; set; }
        public int StockAfter { get; set; }
        [MaxLength(255)]
        public string? Reason { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public virtual Product? Product { get; set; }
    }
}
