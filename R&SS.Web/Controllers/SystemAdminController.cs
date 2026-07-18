using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Activity;
using R_SS.BLL.DTOs.Configuration;
using R_SS.BLL.Interfaces;
using R_SS.Web.Models;

namespace R_SS.Web.Controllers
{
    [Authorize]
    public class SystemAdminController : Controller
    {
        private readonly ISystemActivityLogService _activityLogService;
        private readonly ISystemConfigurationService _configurationService;

        public SystemAdminController(ISystemActivityLogService activityLogService, ISystemConfigurationService configurationService)
        {
            _activityLogService = activityLogService;
            _configurationService = configurationService;
        }

        public async Task<IActionResult> AuditLog(DateTime? fromUtc = null, DateTime? toUtc = null, string? functionName = null, string? operationType = null)
        {
            var search = new SystemActivityLogSearchRequest
            {
                ActorRole = RoleConstants.Manager,
                FromUtc = fromUtc,
                ToUtc = toUtc,
                FunctionName = functionName,
                OperationType = operationType
            };

            return View(new AuditLogViewModel
            {
                Search = search,
                Logs = await _activityLogService.SearchAsync(search)
            });
        }

        public async Task<IActionResult> AuditLogDetails(int id)
        {
            try
            {
                return View(await _activityLogService.GetDetailsAsync(id, RoleConstants.Manager));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(AuditLog));
            }
        }

        public async Task<IActionResult> ProcessManagement(string? groupName = null)
        {
            return View(await BuildProcessManagementViewModelAsync(groupName));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateConfiguration(UpdateSystemConfigurationRequest configuration)
        {
            configuration.ActorUserId = GetActorUserId();
            configuration.ActorRole = RoleConstants.Admin;

            try
            {
                var result = await _configurationService.UpdateAsync(configuration);
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(ProcessManagement), new { groupName = configuration.GroupName });
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(nameof(ProcessManagement), await BuildProcessManagementViewModelAsync(configuration.GroupName, configuration));
            }
        }

        private async Task<ProcessManagementViewModel> BuildProcessManagementViewModelAsync(string? groupName, UpdateSystemConfigurationRequest? configuration = null)
        {
            return new ProcessManagementViewModel
            {
                Configurations = await _configurationService.GetByGroupAsync(groupName, RoleConstants.Admin),
                Configuration = configuration ?? new UpdateSystemConfigurationRequest { GroupName = groupName ?? "Workflow" }
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
    }
}
