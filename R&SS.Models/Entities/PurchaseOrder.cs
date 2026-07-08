using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R_SS.Models.Entities
{
    public class PurchaseOrder
    {
        [Key]
        public int PurchaseOrderId { get; set; }

        [Required, MaxLength(50)]
        public string PurchaseOrderCode { get; set; } = string.Empty;

        public int SupplierId { get; set; }
        public int? CreatedByUserId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ExpectedDate { get; set; }
        public DateTime? ReceivedDate { get; set; }

        [Required, MaxLength(30)]
        public string Status { get; set; } = "Draft";

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual Supplier? Supplier { get; set; }
        public virtual User? CreatedByUser { get; set; }
        public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();
    }
}
