using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required, MaxLength(50)]
        public string SupplierCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string SupplierName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContactName { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? TaxCode { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}
