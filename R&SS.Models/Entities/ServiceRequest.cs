using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class ServiceRequest
    {
        [Key]
        public int ServiceRequestId { get; set; }
        public int CustomerId { get; set; }
        public int? RepairOrderId { get; set; }
        [Required, MaxLength(50)]
        public string RequestCode { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string ServiceType { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string DeviceType { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Brand { get; set; } = string.Empty;
        [MaxLength(150)]
        public string? DeviceModel { get; set; }
        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        [Required, MaxLength(30)]
        public string Status { get; set; } = "Pending Reception";
        public bool NeedsManualProcessing { get; set; }
        [MaxLength(1000)]
        public string? ImageUrls { get; set; }
        [MaxLength(255)]
        public string? CancelReason { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public virtual Customer? Customer { get; set; }
        public virtual RepairOrder? RepairOrder { get; set; }
    }
}
