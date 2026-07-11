using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class PromotionManagementHistory
    {
        [Key]
        public int PromotionManagementHistoryId { get; set; }
        public int PromotionId { get; set; }
        public int ActorUserId { get; set; }
        [Required, MaxLength(30)]
        public string Operation { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? ChangedContent { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public virtual Promotion? Promotion { get; set; }
    }
}
