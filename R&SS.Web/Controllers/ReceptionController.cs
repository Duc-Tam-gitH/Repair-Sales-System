using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.BLL.DTOs.Customer;
using R_SS.BLL.DTOs.ServiceRequest;
using R_SS.BLL.DTOs.TechnicalOrder;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Web.Models;

namespace R_SS.Web.Controllers
{
    [Authorize]
    public class ReceptionController : Controller
    {
        private const string ReceptionRole = "Receptionist";

        private readonly ICustomerService _customerService;
        private readonly IServiceRequestService _serviceRequestService;
        private readonly ITechnicalTicketService _technicalTicketService;
        private readonly IUnitOfWork _unitOfWork;

        public ReceptionController(
            ICustomerService customerService,
            IServiceRequestService serviceRequestService,
            ITechnicalTicketService technicalTicketService,
            IUnitOfWork unitOfWork)
        {
            _customerService = customerService;
            _serviceRequestService = serviceRequestService;
            _technicalTicketService = technicalTicketService;
            _unitOfWork = unitOfWork;
        }

        public IActionResult ReceptionDashboard() => View();

        public async Task<IActionResult> CustomerList(string? keyword = null)
        {
            var model = await _customerService.SearchAsync(new CustomerSearchRequest
            {
                Keyword = keyword,
                ActorRole = GetActorRole()
            });
            ViewData["Keyword"] = keyword;
            return View(model);
        }

        [HttpGet]
        public IActionResult AddCustomer()
        {
            return View(new CreateCustomerRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCustomer(CreateCustomerRequest request)
        {
            request.ActorUserId = GetActorUserId();
            request.ActorRole = GetActorRole();

            try
            {
                var response = await _customerService.CreateAsync(request);
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction(nameof(CustomerDetails), new { id = response.CustomerId });
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(request);
            }
        }

        public async Task<IActionResult> CustomerDetails(int id)
        {
            try
            {
                var model = await _customerService.GetByIdAsync(id, GetActorRole());
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = GetErrorMessage(ex);
                return RedirectToAction(nameof(CustomerList));
            }
        }

        public async Task<IActionResult> RepairRequestList(string? keyword = null)
        {
            var pending = await _unitOfWork.ServiceRequests.GetPendingReceptionAsync();
            var mapped = pending.Select(MapServiceRequest).ToArray();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                mapped = mapped
                    .Where(request =>
                        request.RequestCode.Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        request.DeviceType.Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        request.Brand.Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }

            ViewData["Keyword"] = keyword;
            return View(mapped);
        }

        [HttpGet]
        public async Task<IActionResult> EquipmentReception(int? requestId = null)
        {
            return View(await BuildIntakeModelAsync(requestId, new CreateTechnicalTicketRequest
            {
                ActorRole = GetActorRole(),
                ReceivedByUserId = GetActorUserId(),
                RequestType = "Repair"
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EquipmentReception(CreateTechnicalTicketRequest request)
        {
            request.ActorRole = GetActorRole();
            request.ReceivedByUserId = GetActorUserId();

            try
            {
                var response = await _technicalTicketService.CreateAsync(request);
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction(nameof(TechnicalReportDetails), new { id = response.RepairOrderId });
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(await BuildIntakeModelAsync(null, request));
            }
        }

        public IActionResult CreateTechnicalReport()
        {
            return RedirectToAction(nameof(EquipmentReception));
        }

        public async Task<IActionResult> TechnicalReportList()
        {
            var model = await _technicalTicketService.GetTicketsAsync(BuildTicketViewer());
            return View(model);
        }

        public async Task<IActionResult> TechnicalReportDetails(int id)
        {
            try
            {
                var model = await _technicalTicketService.GetDetailsAsync(id, BuildTicketViewer());
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = GetErrorMessage(ex);
                return RedirectToAction(nameof(TechnicalReportList));
            }
        }

        public async Task<IActionResult> DeviceHandover()
        {
            var model = await _technicalTicketService.GetTicketsAsync(BuildTicketViewer());
            return View(model.Tickets.Where(ticket =>
                ticket.Status.Equals("Pending Delivery", StringComparison.OrdinalIgnoreCase) ||
                ticket.Status.Equals("Pending Manual Delivery", StringComparison.OrdinalIgnoreCase) ||
                ticket.Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase)).ToArray());
        }

        private async Task<ReceptionIntakeViewModel> BuildIntakeModelAsync(int? requestId, CreateTechnicalTicketRequest ticket)
        {
            var customers = await _customerService.SearchAsync(new CustomerSearchRequest
            {
                ActorRole = GetActorRole()
            });

            ServiceRequestResponse? sourceRequest = null;
            if (requestId.HasValue)
            {
                var entity = await _unitOfWork.ServiceRequests.GetByIdAsync(requestId.Value);
                if (entity is not null)
                {
                    sourceRequest = MapServiceRequest(entity);
                    ticket.CustomerId = entity.CustomerId;
                    ticket.DeviceType = entity.DeviceType;
                    ticket.Brand = entity.Brand;
                    ticket.DeviceModel = entity.DeviceModel;
                    ticket.RequestType = entity.ServiceType;
                    ticket.IssueDescription = entity.Description;
                }
            }

            return new ReceptionIntakeViewModel
            {
                Customers = customers.Customers,
                SourceRequest = sourceRequest,
                Ticket = ticket
            };
        }

        private ViewTechnicalTicketsRequest BuildTicketViewer()
        {
            return new ViewTechnicalTicketsRequest
            {
                ActorUserId = GetActorUserId(),
                ActorRole = GetActorRole()
            };
        }

        private int GetActorUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
        }

        private string GetActorRole()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            return string.IsNullOrWhiteSpace(role) || role.Equals("Reception", StringComparison.OrdinalIgnoreCase)
                ? ReceptionRole
                : role;
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

        private static ServiceRequestResponse MapServiceRequest(R_SS.Models.Entities.ServiceRequest request)
        {
            return new ServiceRequestResponse
            {
                ServiceRequestId = request.ServiceRequestId,
                RequestCode = request.RequestCode,
                CustomerId = request.CustomerId,
                ServiceType = request.ServiceType,
                DeviceType = request.DeviceType,
                Brand = request.Brand,
                DeviceModel = request.DeviceModel,
                Description = request.Description,
                Status = request.Status,
                NeedsManualProcessing = request.NeedsManualProcessing,
                ImageFileNames = string.IsNullOrWhiteSpace(request.ImageUrls)
                    ? Array.Empty<string>()
                    : request.ImageUrls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            };
        }
    }
}
