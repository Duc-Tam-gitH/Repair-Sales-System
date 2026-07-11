using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R_SS.Models.Entities
{
    public class Promotion
    {
        [Key]
        public int PromotionId { get; set; }

        [Required, MaxLength(50)]
        public string PromotionCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string ProgramName { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string PromotionType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PromotionValue { get; set; }

        public DateTime StartDateUtc { get; set; }

        public DateTime EndDateUtc { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();
    }
}
