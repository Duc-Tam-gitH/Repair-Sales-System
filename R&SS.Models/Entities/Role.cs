using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R_SS.Models.Entities
{
    public class Role
{
    public int RoleId { get; set; }
    public required string RoleName { get; set; }
    public string? Description { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
}
