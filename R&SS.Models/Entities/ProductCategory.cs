using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class ProductCategory
    {
        [Key]
        public int ProductCategoryId { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string CategoryCode { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
