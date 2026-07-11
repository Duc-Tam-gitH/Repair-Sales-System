using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R_SS.Models.Entities
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required, MaxLength(50)]
        public string ProductCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string ProductName { get; set; } = string.Empty;

        public int ProductCategoryId { get; set; }
        public int? SupplierId { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }

        public int QuantityInStock { get; set; }
        public int ReorderLevel { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ProductCategory? ProductCategory { get; set; }
        public virtual Supplier? Supplier { get; set; }
        public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();
        public virtual ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();
        public virtual ICollection<RepairOrderDetail> RepairOrderDetails { get; set; } = new List<RepairOrderDetail>();
    }
}
