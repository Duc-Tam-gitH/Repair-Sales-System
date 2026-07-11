using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class ProductCategoryManagementHistory
    {
        [Key]
        public int ProductCategoryManagementHistoryId { get; set; }
        public int ProductCategoryId { get; set; }
        public int ActorUserId { get; set; }
        [Required, MaxLength(30)]
        public string Operation { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? ChangedContent { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public virtual ProductCategory? ProductCategory { get; set; }
    }
}
