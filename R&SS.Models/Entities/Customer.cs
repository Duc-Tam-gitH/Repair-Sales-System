using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required, MaxLength(50)]
        public string CustomerCode { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
        public virtual ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<CustomerUpdateHistory> UpdateHistories { get; set; } = new List<CustomerUpdateHistory>();
    }
}
