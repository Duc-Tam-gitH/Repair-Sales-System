using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class ServiceFeedback
    {
        [Key]
        public int ServiceFeedbackId { get; set; }
        public int CustomerId { get; set; }
        public int? SalesOrderId { get; set; }
        public int? RepairOrderId { get; set; }
        public int Rating { get; set; }
        [MaxLength(1000)]
        public string? Comment { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public virtual Customer? Customer { get; set; }
        public virtual SalesOrder? SalesOrder { get; set; }
        public virtual RepairOrder? RepairOrder { get; set; }
    }
}
