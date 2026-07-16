using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Authentication;
using R_SS.BLL.Interfaces;
using System.Security.Claims;

namespace R_SS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        return Ok(new ApiResponse<RegisterResponse>
        {
            Success = true,
            Message = "Register successfully.",
            Data = result
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        return Ok(new ApiResponse<LoginResponse>
        {
            Success = true,
            Message = "Login successfully.",
            Data = result
        });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password/request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.RequestPasswordResetAsync(request);

        return Ok(new ApiResponse<ForgotPasswordResponse>
        {
            Success = true,
            Message = "OTP sent successfully.",
            Data = result
        });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password/verify")]
    public async Task<IActionResult> VerifyPasswordResetOtp([FromBody] VerifyForgotPasswordOtpRequest request)
    {
        var result = await _authService.VerifyPasswordResetOtpAsync(request);

        return Ok(new ApiResponse<VerifyForgotPasswordOtpResponse>
        {
            Success = true,
            Message = "OTP verified successfully.",
            Data = result
        });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);

        return Ok(new ApiResponse<ResetPasswordResponse>
        {
            Success = true,
            Message = "Password reset successfully.",
            Data = result
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var result = await _authService.LogoutAsync();

        return Ok(new ApiResponse<LogoutResponse>
        {
            Success = true,
            Message = "Logout successfully.",
            Data = result
        });
    }

    [Authorize(Roles = "Client")]
    [HttpGet("client-only")]
    public IActionResult ClientOnly()
    {
        return Ok("Client access granted.");
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new ApiResponse<ChangePasswordResponse>
            {
                Success = false,
                Message = "Unauthorized.",
                Data = null
            });
        }

        request.UserId = userId.Value;
        var result = await _authService.ChangePasswordAsync(request);

        return Ok(new ApiResponse<ChangePasswordResponse>
        {
            Success = true,
            Message = "Password changed successfully.",
            Data = result
        });
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdatePersonalInfo([FromBody] UpdatePersonalInfoRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new ApiResponse<PersonalInfoResponse>
            {
                Success = false,
                Message = "Unauthorized.",
                Data = null
            });
        }

        request.UserId = userId.Value;
        var result = await _authService.UpdatePersonalInfoAsync(request);

        return Ok(new ApiResponse<PersonalInfoResponse>
        {
            Success = true,
            Message = "Personal information updated successfully.",
            Data = result
        });
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
