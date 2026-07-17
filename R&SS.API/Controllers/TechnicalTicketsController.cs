using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.TechnicalOrder;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/technical-tickets")]
public class TechnicalTicketsController : AuthenticatedControllerBase
{
    private readonly ITechnicalTicketService _technicalTicketService;

    public TechnicalTicketsController(ITechnicalTicketService technicalTicketService)
    {
        _technicalTicketService = technicalTicketService;
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
}
