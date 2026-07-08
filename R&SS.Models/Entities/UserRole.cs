using R_SS.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R_SS.Models.Entities
{
    public class UserRole
{
    public int UserRoleId { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public required virtual User User { get; set; }
    public required virtual Role Role { get; set; }
}
}
