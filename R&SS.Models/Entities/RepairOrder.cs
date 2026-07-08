using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R_SS.Models.Entities
{
    public class RepairOrder
    {
        [Key]
        public int RepairOrderId { get; set; }

        [Required, MaxLength(50)]
        public string RepairOrderCode { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public int? ReceivedByUserId { get; set; }

        [MaxLength(150)]
        public string? DeviceName { get; set; }

        [MaxLength(150)]
        public string? DeviceModel { get; set; }

        [MaxLength(150)]
        public string? SerialNumber { get; set; }

        [Required, MaxLength(255)]
        public string IssueDescription { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Diagnosis { get; set; }

        public DateTime ReceivedDate { get; set; } = DateTime.Now;
        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        [Required, MaxLength(30)]
        public string Status { get; set; } = "Received";

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual Customer? Customer { get; set; }
        public virtual User? ReceivedByUser { get; set; }
        public virtual ICollection<RepairOrderDetail> RepairOrderDetails { get; set; } = new List<RepairOrderDetail>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
