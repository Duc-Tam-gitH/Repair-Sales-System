using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        public int UserId { get; set; }

        public int RoleId { get; set; }

        [Required, MaxLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

        [MaxLength(30)]
        public string WorkStatus { get; set; } = "Working";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual User? User { get; set; }

        public virtual Role? Role { get; set; }
    }
}
