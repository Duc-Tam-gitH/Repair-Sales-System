using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class SystemActivityLog
    {
        [Key]
        public int SystemActivityLogId { get; set; }
        public int? ActorUserId { get; set; }
        [MaxLength(100)]
        public string? ActorUsername { get; set; }
        [Required, MaxLength(100)]
        public string FunctionName { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string OperationType { get; set; } = string.Empty;
        [MaxLength(255)]
        public string? AffectedData { get; set; }
        [MaxLength(50)]
        public string ExecutionResult { get; set; } = "Success";
        [MaxLength(50)]
        public string? IpAddress { get; set; }
        [MaxLength(1000)]
        public string? Details { get; set; }
        public DateTime ExecutedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
