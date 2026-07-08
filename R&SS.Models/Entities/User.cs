using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R_SS.Models.Entities
{
    public class User
{
    [Key]
    public int UserId { get; set; }
    [Required, MaxLength(50)]
    public required string Username { get; set; }
    [Required, MaxLength(255)]
    public required string PasswordHash { get; set; }
    [Required, MaxLength(100)]
    public required string Email { get; set; }
    [Required, MaxLength(100)]
    public required string FullName { get; set; }
    [MaxLength(20)]
    public string? Phone { get; set; }
    [MaxLength(255)]
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
}
