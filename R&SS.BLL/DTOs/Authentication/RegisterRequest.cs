using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace R_SS.BLL.DTOs.Authentication;

public class RegisterRequest
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ và tên không được để trống.")]
    public string FullName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Address { get; set; }
}
