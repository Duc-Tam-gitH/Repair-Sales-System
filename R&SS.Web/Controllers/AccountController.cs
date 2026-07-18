using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Authentication;
using R_SS.BLL.Interfaces;

namespace R_SS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleHome(User.FindFirstValue(ClaimTypes.Role));
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request, bool rememberMe = false, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(request);
            }

            try
            {
                var response = await _authService.LoginAsync(request);
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, response.UserId.ToString()),
                    new(ClaimTypes.Name, response.FullName),
                    new(ClaimTypes.Email, response.Email),
                    new(ClaimTypes.Role, response.RoleName),
                    new("AccessToken", response.AccessToken)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var properties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = response.ExpiresAtUtc
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    properties);

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToRoleHome(response.RoleName);
            }
            catch (Exception ex)
            {
                AddError(ex);
                ViewData["ReturnUrl"] = returnUrl;
                return View(request);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                await _authService.RegisterAsync(request);
                TempData["SuccessMessage"] = "Account created successfully. Please log in.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(request);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                await _authService.RequestPasswordResetAsync(request);
                TempData["SuccessMessage"] = "OTP sent successfully. Check the app logs in local development.";
                return RedirectToAction(nameof(VerifyOTP), new { email = request.Email });
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(request);
            }
        }

        [HttpGet]
        public IActionResult VerifyOTP(string? email)
        {
            return View(new VerifyForgotPasswordOtpRequest { Email = email ?? string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(VerifyForgotPasswordOtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                await _authService.VerifyPasswordResetOtpAsync(request);
                TempData["SuccessMessage"] = "OTP verified. You can now update your password.";
                return RedirectToAction(nameof(ResetPassword), new { email = request.Email });
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(request);
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string? email)
        {
            return View(new ResetPasswordRequest { Email = email ?? string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                await _authService.ResetPasswordAsync(request);
                TempData["SuccessMessage"] = "Password reset successfully. Please log in.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(request);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> PersonalInfo()
        {
            try
            {
                var userId = GetCurrentUserId();
                var model = await _authService.GetPersonalInfoAsync(userId);
                return View(model);
            }
            catch (Exception ex)
            {
                AddError(ex);
                return RedirectToAction(nameof(Login));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> PersonalInfo(UpdatePersonalInfoRequest request)
        {
            request.UserId = GetCurrentUserId();
            if (!ModelState.IsValid)
            {
                return View(ToPersonalInfoResponse(request));
            }

            try
            {
                var model = await _authService.UpdatePersonalInfoAsync(request);
                TempData["SuccessMessage"] = model.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(ToPersonalInfoResponse(request));
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            request.UserId = GetCurrentUserId();
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                await _authService.ChangePasswordAsync(request);
                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction(nameof(PersonalInfo));
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(request);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        private int GetCurrentUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
                ? userId
                : 0;
        }

        private IActionResult RedirectToRoleHome(string? roleName)
        {
            return roleName switch
            {
                RoleConstants.Customer => RedirectToAction("CustomerHome", "Customer"),
                RoleConstants.Receptionist => RedirectToAction("ReceptionDashboard", "Reception"),
                RoleConstants.Technician => RedirectToAction("TechnicianDashboard", "Technician"),
                RoleConstants.Manager or RoleConstants.Admin => RedirectToAction("ManagementDashboard", "Management"),
                _ => RedirectToAction("Home", "Home")
            };
        }

        private void AddError(Exception exception)
        {
            if (exception is ValidationException validationException)
            {
                foreach (var error in validationException.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return;
            }

            ModelState.AddModelError(string.Empty, exception.Message);
        }

        private static PersonalInfoResponse ToPersonalInfoResponse(UpdatePersonalInfoRequest request)
        {
            return new PersonalInfoResponse
            {
                UserId = request.UserId,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address
            };
        }
    }
}
