using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class InvoiceRecord
    {
        [Key]
        public int InvoiceRecordId { get; set; }
        [Required, MaxLength(50)]
        public string InvoiceCode { get; set; } = string.Empty;
        public int? SalesOrderId { get; set; }
        public int? RepairOrderId { get; set; }
        public int ActorUserId { get; set; }
        [Required, MaxLength(30)]
        public string InvoiceType { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? PdfPath { get; set; }
        [MaxLength(100)]
        public string? SentToEmail { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public virtual SalesOrder? SalesOrder { get; set; }
        public virtual RepairOrder? RepairOrder { get; set; }
    }
}
