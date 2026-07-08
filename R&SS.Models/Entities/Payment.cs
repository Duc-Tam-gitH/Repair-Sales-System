using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R_SS.Models.Entities
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required, MaxLength(50)]
        public string PaymentCode { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public int? SalesOrderId { get; set; }
        public int? RepairOrderId { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required, MaxLength(30)]
        public string PaymentMethod { get; set; } = "Cash";

        [Required, MaxLength(30)]
        public string PaymentStatus { get; set; } = "Completed";

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual Customer? Customer { get; set; }
        public virtual SalesOrder? SalesOrder { get; set; }
        public virtual RepairOrder? RepairOrder { get; set; }
    }
}
