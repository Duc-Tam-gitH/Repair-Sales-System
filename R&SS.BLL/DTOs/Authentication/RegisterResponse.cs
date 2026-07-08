using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R_SS.BLL.DTOs.Authentication;

public class RegisterResponse
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
