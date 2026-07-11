using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class RolePermission
    {
        [Key]
        public int RolePermissionId { get; set; }
        public int RoleId { get; set; }
        [Required, MaxLength(20)]
        public string UseCaseId { get; set; } = string.Empty;
        [Required, MaxLength(150)]
        public string FunctionName { get; set; } = string.Empty;
        public virtual Role? Role { get; set; }
    }
}
