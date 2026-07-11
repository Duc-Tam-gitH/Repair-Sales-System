using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class RepairOrderStatusHistory
    {
        [Key]
        public int RepairOrderStatusHistoryId { get; set; }
        public int RepairOrderId { get; set; }
        public int UpdatedByUserId { get; set; }
        [Required, MaxLength(30)]
        public string Status { get; set; } = string.Empty;
        [MaxLength(255)]
        public string? Notes { get; set; }
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public virtual RepairOrder? RepairOrder { get; set; }
    }
}
