using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R_SS.Models.Entities
{
    public class RepairOrderDetail
    {
        [Key]
        public int RepairOrderDetailId { get; set; }

        public int RepairOrderId { get; set; }
        public int? ProductId { get; set; }

        [Required, MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        public virtual RepairOrder? RepairOrder { get; set; }
        public virtual Product? Product { get; set; }
    }
}
