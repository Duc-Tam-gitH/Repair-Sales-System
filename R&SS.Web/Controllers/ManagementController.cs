using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Account;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Web.Models;

namespace R_SS.Web.Controllers
{
    [Authorize]
    public class ManagementController : Controller
    {
        private readonly IAccountManagementService _accountManagementService;
        private readonly IUnitOfWork _unitOfWork;

        public ManagementController(IAccountManagementService accountManagementService, IUnitOfWork unitOfWork)
        {
            _accountManagementService = accountManagementService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> ManagementDashboard() => View(await BuildManagementViewModelAsync());
        public async Task<IActionResult> RepairProgressDashboard() => View(await BuildManagementViewModelAsync());
        public async Task<IActionResult> SystemAccountManagement() => View(await BuildManagementViewModelAsync());
        public async Task<IActionResult> AddAccount(int? id = null) => View(await BuildManagementViewModelAsync(await BuildAccountRequestAsync(id)));
        public async Task<IActionResult> RoleAuthorization() => View(await BuildManagementViewModelAsync());
        public async Task<IActionResult> EmployeeManagement() => View(await BuildManagementViewModelAsync());
        public async Task<IActionResult> CustomerManagement() => View(await BuildManagementViewModelAsync());
        public async Task<IActionResult> TechnicalRequestManagement() => View(await BuildManagementViewModelAsync());
        public async Task<IActionResult> ApproveQuotes() => View(await BuildManagementViewModelAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAccount(ManageAccountRequest account)
        {
            account.ActorUserId = GetActorUserId();
            account.ActorRole = RoleConstants.Admin;

            try
            {
                var result = account.UserId.HasValue
                    ? await _accountManagementService.UpdateAsync(account)
                    : await _accountManagementService.AddAsync(account);

                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(SystemAccountManagement));
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(await BuildManagementViewModelAsync(account));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAccountLock(int userId, bool isLocked)
        {
            try
            {
                var result = await _accountManagementService.SetLockAsync(userId, GetActorUserId(), RoleConstants.Admin, isLocked);
                TempData["SuccessMessage"] = result.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = GetErrorMessage(ex);
            }

            return RedirectToAction(nameof(SystemAccountManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(int userId)
        {
            try
            {
                var result = await _accountManagementService.DeleteAsync(userId, GetActorUserId(), RoleConstants.Admin);
                TempData["SuccessMessage"] = result.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = GetErrorMessage(ex);
            }

            return RedirectToAction(nameof(SystemAccountManagement));
        }

        private async Task<ManagementViewModel> BuildManagementViewModelAsync(ManageAccountRequest? account = null)
        {
            return new ManagementViewModel
            {
                Users = await _unitOfWork.Users.GetAllWithRolesAsync(),
                Roles = (await _unitOfWork.Roles.GetAllAsync()).OrderBy(role => role.RoleName).ToArray(),
                Employees = (await _unitOfWork.Employees.GetAllAsync()).OrderBy(employee => employee.FullName).ToArray(),
                Customers = (await _unitOfWork.Customers.GetAllAsync()).OrderBy(customer => customer.FullName).ToArray(),
                ServiceRequests = (await _unitOfWork.ServiceRequests.GetAllAsync()).OrderByDescending(request => request.CreatedAtUtc).ToArray(),
                RepairOrders = await _unitOfWork.RepairOrders.GetVisibleTicketsAsync(RoleConstants.Manager, GetActorUserId(), null),
                SalesOrders = await _unitOfWork.SalesOrders.GetAllWithDetailsAsync(),
                Account = account ?? new ManageAccountRequest { IsActive = true }
            };
        }

        private async Task<ManageAccountRequest?> BuildAccountRequestAsync(int? userId)
        {
            if (!userId.HasValue)
            {
                return new ManageAccountRequest { IsActive = true };
            }

            var user = (await _unitOfWork.Users.GetAllWithRolesAsync()).FirstOrDefault(item => item.UserId == userId.Value);
            if (user is null)
            {
                return new ManageAccountRequest { IsActive = true };
            }

            return new ManageAccountRequest
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                RoleName = user.UserRoles.FirstOrDefault()?.Role?.RoleName ?? string.Empty,
                IsActive = user.IsActive
            };
        }

        private int GetActorUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
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

        private static string GetErrorMessage(Exception exception)
        {
            return exception is ValidationException validationException
                ? string.Join(" ", validationException.Errors.Select(error => error.ErrorMessage))
                : exception.Message;
        }
    }
}
