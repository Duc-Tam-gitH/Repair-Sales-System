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
        public int? AssignedTechnicianId { get; set; }

        [MaxLength(150)]
        public string? DeviceName { get; set; }

        [MaxLength(100)]
        public string DeviceType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? DeviceModel { get; set; }

        [MaxLength(150)]
        public string? SerialNumber { get; set; }

        [Required, MaxLength(255)]
        public string IssueDescription { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string RequestType { get; set; } = "Repair";

        [MaxLength(255)]
        public string? DeviceCondition { get; set; }

        [MaxLength(255)]
        public string? Diagnosis { get; set; }

        [MaxLength(255)]
        public string? InspectionResult { get; set; }

        [MaxLength(255)]
        public string? WorkPerformed { get; set; }

        [MaxLength(255)]
        public string? RepairResult { get; set; }

        [MaxLength(255)]
        public string? AccompanyingAccessories { get; set; }

        public int? ProcessingMinutes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ServiceFee { get; set; }

        public DateTime ReceivedDate { get; set; } = DateTime.Now;
        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? PendingDeliveryConfirmationAtUtc { get; set; }
        public DateTime? DeliveryConfirmationDeadlineUtc { get; set; }
        public DateTime? DeliveryConfirmedAtUtc { get; set; }

        [MaxLength(30)]
        public string? DeliveryConfirmationMethod { get; set; }

        public DateTime? DeliveryOtpSentAtUtc { get; set; }
        [MaxLength(255)]
        public string? DeliveryOtpHash { get; set; }
        public DateTime? DeliveryOtpExpiresAtUtc { get; set; }
        public int DeliveryOtpAttemptCount { get; set; }
        public int DeliveryOtpResendCount { get; set; }
        public DateTime? DeliveryConfirmationLockedUntilUtc { get; set; }
        public int DeliveryRejectionCount { get; set; }
        [MaxLength(50)]
        public string? DeliveryConfirmationIpAddress { get; set; }

        [MaxLength(100)]
        public string? DeliveryOtpSentToEmail { get; set; }

        [MaxLength(20)]
        public string? DeliveryOtpSentToPhone { get; set; }

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
        public virtual User? AssignedTechnician { get; set; }
        public virtual ICollection<RepairOrderDetail> RepairOrderDetails { get; set; } = new List<RepairOrderDetail>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<RepairOrderStatusHistory> StatusHistories { get; set; } = new List<RepairOrderStatusHistory>();
        public virtual ICollection<TechnicianAssignmentHistory> AssignmentHistories { get; set; } = new List<TechnicianAssignmentHistory>();
    }
}
