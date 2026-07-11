using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        public int CustomerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual Customer? Customer { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
