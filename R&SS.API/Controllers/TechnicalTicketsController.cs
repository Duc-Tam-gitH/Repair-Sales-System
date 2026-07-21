using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Delivery;
using R_SS.BLL.DTOs.TechnicalOrder;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/technical-tickets")]
[Route("api/technical-orders")]
public class TechnicalTicketsController : AuthenticatedControllerBase
{
    private readonly ITechnicalTicketService _technicalTicketService;
    private readonly IDeliveryConfirmationService _deliveryConfirmationService;

    public TechnicalTicketsController(ITechnicalTicketService technicalTicketService, IDeliveryConfirmationService deliveryConfirmationService)
    {
        _technicalTicketService = technicalTicketService;
        _deliveryConfirmationService = deliveryConfirmationService;
    }

    [Authorize(Roles = "Receptionist,Manager")]
    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTechnicalTicketRequest request)
    {
        request.ActorRole = CurrentRole();
        request.ReceivedByUserId = CurrentUserId();
        var result = await _technicalTicketService.CreateAsync(request);
        return Ok(new ApiResponse<TechnicalTicketResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetTickets([FromQuery] int? customerId)
    {
        var result = await _technicalTicketService.GetTicketsAsync(new ViewTechnicalTicketsRequest
        {
            ActorRole = CurrentRole(),
            ActorUserId = CurrentUserId(),
            CustomerId = customerId
        });
        return Ok(new ApiResponse<TechnicalTicketListResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTicket(int id, [FromQuery] int? customerId)
    {
        var result = await _technicalTicketService.GetDetailsAsync(id, new ViewTechnicalTicketsRequest
        {
            ActorRole = CurrentRole(),
            ActorUserId = CurrentUserId(),
            CustomerId = customerId
        });
        return Ok(new ApiResponse<TechnicalTicketResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpGet("{id:int}/progress")]
    public async Task<IActionResult> TrackProgress(int id, [FromQuery] int? customerId)
    {
        var result = await _technicalTicketService.TrackProgressAsync(id, new ViewTechnicalTicketsRequest
        {
            ActorRole = CurrentRole(),
            ActorUserId = CurrentUserId(),
            CustomerId = customerId
        });
        return Ok(new ApiResponse<TechnicalTicketProgressResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Receptionist,Manager")]
    [HttpGet("technicians")]
    public async Task<IActionResult> GetTechnicians()
    {
        var result = await _technicalTicketService.GetTechniciansAsync(CurrentRole());
        return Ok(new ApiResponse<TechnicianListResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Receptionist,Manager")]
    [HttpPost("{id:int}/assign-technician")]
    [HttpPost("{id:int}/assign")]
    public async Task<IActionResult> AssignTechnician(int id, [FromBody] AssignTechnicianRequest request)
    {
        request.RepairOrderId = id;
        request.ActorRole = CurrentRole();
        request.AssignedByUserId = CurrentUserId();
        var result = await _technicalTicketService.AssignTechnicianAsync(request);
        return Ok(new ApiResponse<TechnicalTicketResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Technician,Receptionist,Manager")]
    [HttpPut("{id:int}")]
    [HttpPut("{id:int}/reassign")]
    public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTechnicalTicketRequest request)
    {
        request.RepairOrderId = id;
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _technicalTicketService.UpdateAsync(request);
        return Ok(new ApiResponse<TechnicalTicketResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Receptionist,Manager")]
    [HttpPost("{id:int}/payment")]
    public async Task<IActionResult> ConfirmPayment(int id, [FromBody] ConfirmRepairPaymentRequest request)
    {
        request.RepairOrderId = id;
        request.ActorRole = CurrentRole();
        request.ConfirmedByUserId = CurrentUserId();
        var result = await _technicalTicketService.ConfirmPaymentAsync(request);
        return Ok(new ApiResponse<TechnicalTicketResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Technician,Receptionist,Manager")]
    [HttpPost("{id:int}/progress")]
    public async Task<IActionResult> UpdateProgress(int id, [FromBody] UpdateTechnicalTicketRequest request)
    {
        request.RepairOrderId = id;
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _technicalTicketService.UpdateAsync(request);
        return Ok(new ApiResponse<TechnicalTicketResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Technician,Receptionist,Manager")]
    [HttpPost("{id:int}/parts")]
    public async Task<IActionResult> AddParts(int id, [FromBody] TechnicalOrderPartsRequest request)
    {
        var update = new UpdateTechnicalTicketRequest
        {
            RepairOrderId = id,
            ActorRole = CurrentRole(),
            ActorUserId = CurrentUserId(),
            UsedComponents = request.Parts
        };
        var result = await _technicalTicketService.UpdateAsync(update);
        return Ok(new ApiResponse<TechnicalTicketResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Technician,Receptionist,Manager")]
    [HttpDelete("{id:int}/parts/{partId:int}")]
    public IActionResult DeletePart(int id, int partId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new ApiResponse<object>
        {
            Success = false,
            Message = "Deleting a single used part is not implemented by the current technical ticket service."
        });
    }

    [Authorize(Roles = "Technician,Receptionist,Manager")]
    [HttpPatch("{id:int}/complete-repair")]
    public async Task<IActionResult> CompleteRepair(int id, [FromBody] UpdateTechnicalTicketRequest request)
    {
        request.RepairOrderId = id;
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        request.StatusDecision = string.IsNullOrWhiteSpace(request.StatusDecision) ? "Completed" : request.StatusDecision;
        var result = await _technicalTicketService.UpdateAsync(request);
        return Ok(new ApiResponse<TechnicalTicketResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPost("{id:int}/send-handover-otp")]
    public async Task<IActionResult> SendHandoverOtp(int id, [FromBody] ResendDeliveryOtpRequest request)
    {
        request.RepairOrderId = id;
        var result = await _deliveryConfirmationService.ResendOtpAsync(request);
        return Ok(new ApiResponse<DeliveryConfirmationResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPost("{id:int}/verify-handover-otp")]
    public async Task<IActionResult> VerifyHandoverOtp(int id, [FromBody] ConfirmDeliveryRequest request)
    {
        request.RepairOrderId = id;
        request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _deliveryConfirmationService.ConfirmAsync(request);
        return Ok(new ApiResponse<DeliveryConfirmationResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id, [FromQuery] int? customerId)
    {
        var result = await _technicalTicketService.TrackProgressAsync(id, new ViewTechnicalTicketsRequest
        {
            ActorRole = CurrentRole(),
            ActorUserId = CurrentUserId(),
            CustomerId = customerId
        });
        return Ok(new ApiResponse<TechnicalTicketProgressResponse> { Success = true, Message = result.Message, Data = result });
    }

    [Authorize(Roles = "Receptionist,Manager")]
    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> CancelTicket(int id, [FromBody] CancelTechnicalTicketRequest request)
    {
        request.RepairOrderId = id;
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _technicalTicketService.CancelAsync(request);
        return Ok(new ApiResponse<TechnicalTicketResponse> { Success = true, Message = result.Message, Data = result });
    }

    public sealed class TechnicalOrderPartsRequest
    {
        public IReadOnlyCollection<UsedComponentRequest> Parts { get; set; } = Array.Empty<UsedComponentRequest>();
    }
}
