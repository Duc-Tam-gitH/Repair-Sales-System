using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.TechnicalOrder;
using R_SS.BLL.Interfaces;
using R_SS.Web.Models;

namespace R_SS.Web.Controllers
{
    [Authorize]
    public class TechnicianController : Controller
    {
        private readonly ITechnicalTicketService _technicalTicketService;

        public TechnicianController(ITechnicalTicketService technicalTicketService)
        {
            _technicalTicketService = technicalTicketService;
        }

        public async Task<IActionResult> TechnicianDashboard()
        {
            return View(await GetAssignedTicketsAsync());
        }

        public async Task<IActionResult> AssignedTasks()
        {
            return View(await GetAssignedTicketsAsync());
        }

        public async Task<IActionResult> TaskDetails(int id)
        {
            return View(await BuildTaskViewModelAsync(id));
        }

        [HttpGet]
        public async Task<IActionResult> RecordFaults(int id)
        {
            return View(await BuildTaskViewModelAsync(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordFaults(UpdateTechnicalTicketRequest update)
        {
            update.ActorUserId = GetActorUserId();
            update.ActorRole = RoleConstants.Technician;

            try
            {
                await _technicalTicketService.UpdateAsync(update);
                TempData["SuccessMessage"] = "Inspection results saved successfully.";
                return RedirectToAction(nameof(TaskDetails), new { id = update.RepairOrderId });
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(await BuildTaskViewModelAsync(update.RepairOrderId, update));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProposeRepairPlan(int id)
        {
            return View(await BuildTaskViewModelAsync(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProposeRepairPlan(UpdateTechnicalTicketRequest update)
        {
            update.ActorUserId = GetActorUserId();
            update.ActorRole = RoleConstants.Technician;

            try
            {
                await _technicalTicketService.UpdateAsync(update);
                TempData["SuccessMessage"] = "Repair plan saved successfully.";
                return RedirectToAction(nameof(UpdateProgress), new { id = update.RepairOrderId });
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(await BuildTaskViewModelAsync(update.RepairOrderId, update));
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProgress(int id)
        {
            return View(await BuildTaskViewModelAsync(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgress(UpdateTechnicalTicketRequest update)
        {
            update.ActorUserId = GetActorUserId();
            update.ActorRole = RoleConstants.Technician;

            try
            {
                await _technicalTicketService.UpdateAsync(update);
                TempData["SuccessMessage"] = "Progress updated successfully.";
                return RedirectToAction(nameof(TaskDetails), new { id = update.RepairOrderId });
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(await BuildTaskViewModelAsync(update.RepairOrderId, update));
            }
        }

        public IActionResult ComponentSearch() => View();

        [HttpGet]
        public async Task<IActionResult> UpdateComponents(int id)
        {
            return View(await BuildTaskViewModelAsync(id));
        }

        [HttpGet]
        public async Task<IActionResult> RecordResults(int id)
        {
            return View(await BuildTaskViewModelAsync(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResults(UpdateTechnicalTicketRequest update)
        {
            update.ActorUserId = GetActorUserId();
            update.ActorRole = RoleConstants.Technician;
            update.StatusDecision = "Complete Repair";

            try
            {
                await _technicalTicketService.UpdateAsync(update);
                TempData["SuccessMessage"] = "Task completed and ready for handover.";
                return RedirectToAction(nameof(AssignedTasks));
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(await BuildTaskViewModelAsync(update.RepairOrderId, update));
            }
        }

        public async Task<IActionResult> WorkSchedule()
        {
            return View(await GetAssignedTicketsAsync());
        }

        private async Task<TechnicalTicketListResponse> GetAssignedTicketsAsync()
        {
            return await _technicalTicketService.GetTicketsAsync(new ViewTechnicalTicketsRequest
            {
                ActorUserId = GetActorUserId(),
                ActorRole = RoleConstants.Technician
            });
        }

        private async Task<TechnicianTaskViewModel> BuildTaskViewModelAsync(int id, UpdateTechnicalTicketRequest? update = null)
        {
            var viewer = new ViewTechnicalTicketsRequest
            {
                ActorUserId = GetActorUserId(),
                ActorRole = RoleConstants.Technician
            };
            var progress = await _technicalTicketService.TrackProgressAsync(id, viewer);

            return new TechnicianTaskViewModel
            {
                Ticket = progress.Ticket,
                History = progress.History,
                Update = update ?? new UpdateTechnicalTicketRequest { RepairOrderId = id }
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
