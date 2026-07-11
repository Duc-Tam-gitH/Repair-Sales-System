using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class CustomerUpdateHistory
    {
        [Key]
        public int CustomerUpdateHistoryId { get; set; }

        public int CustomerId { get; set; }

        public int UpdatedByUserId { get; set; }

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(1000)]
        public string ChangedContent { get; set; } = string.Empty;

        public virtual Customer? Customer { get; set; }
    }
}
