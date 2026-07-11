using System.ComponentModel.DataAnnotations.Schema;

namespace R_SS.Models.Entities
{
    public class CartItem
    {
        public int CartItemId { get; set; }

        public int CartId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual Cart? Cart { get; set; }

        public virtual Product? Product { get; set; }
    }
}
