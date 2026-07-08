using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R_SS.Models.Entities
{
    public class PurchaseOrderDetail
    {
        [Key]
        public int PurchaseOrderDetailId { get; set; }

        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        public virtual PurchaseOrder? PurchaseOrder { get; set; }
        public virtual Product? Product { get; set; }
    }
}

