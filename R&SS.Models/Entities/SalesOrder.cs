using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R_SS.Models.Entities
{
    public class SalesOrder
    {
        [Key]
        public int SalesOrderId { get; set; }

        [Required, MaxLength(50)]
        public string SalesOrderCode { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public int? CreatedByUserId { get; set; }

        public DateTime SalesDate { get; set; } = DateTime.Now;

        [Required, MaxLength(30)]
        public string Status { get; set; } = "Pending";

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual Customer? Customer { get; set; }
        public virtual User? CreatedByUser { get; set; }
        public virtual ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
