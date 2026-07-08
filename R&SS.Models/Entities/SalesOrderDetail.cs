using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R_SS.Models.Entities
{
    public class SalesOrderDetail
    {
        [Key]
        public int SalesOrderDetailId { get; set; }

        public int SalesOrderId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        public virtual SalesOrder? SalesOrder { get; set; }
        public virtual Product? Product { get; set; }
    }
}
