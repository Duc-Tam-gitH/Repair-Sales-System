using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R_SS.BLL.DTOs.Authentication;

namespace R_SS.BLL.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration data.</param>
    /// <returns>The registration result.</returns>
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authenticates a user account using email or username plus password.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>The login result, including role and token.</returns>
    Task<LoginResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Requests a password reset OTP for the specified email.
    /// </summary>
    /// <param name="request">Forgot password request data.</param>
    /// <returns>The password reset request result.</returns>
    Task<ForgotPasswordResponse> RequestPasswordResetAsync(ForgotPasswordRequest request);

    /// <summary>
    /// Verifies the OTP that was sent to the user email.
    /// </summary>
    /// <param name="request">OTP verification request data.</param>
    /// <returns>The OTP verification result.</returns>
    Task<VerifyForgotPasswordOtpResponse> VerifyPasswordResetOtpAsync(VerifyForgotPasswordOtpRequest request);

    /// <summary>
    /// Resets the password after OTP verification.
    /// </summary>
    /// <param name="request">Reset password request data.</param>
    /// <returns>The password reset result.</returns>
    Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>
    /// Changes the password for the authenticated user.
    /// </summary>
    /// <param name="request">Change password request data.</param>
    /// <returns>The password change result.</returns>
    Task<ChangePasswordResponse> ChangePasswordAsync(ChangePasswordRequest request);

    /// <summary>
    /// Gets the authenticated user's personal information.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <returns>The user's personal information.</returns>
    Task<PersonalInfoResponse> GetPersonalInfoAsync(int userId);

    /// <summary>
    /// Updates the authenticated user's personal information.
    /// </summary>
    /// <param name="request">Personal information update data.</param>
    /// <returns>The updated personal information.</returns>
    Task<PersonalInfoResponse> UpdatePersonalInfoAsync(UpdatePersonalInfoRequest request);

    /// <summary>
    /// Ends the current authenticated session.
    /// </summary>
    /// <returns>The logout confirmation.</returns>
    Task<LogoutResponse> LogoutAsync();
}
