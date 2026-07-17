using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Customer;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize(Roles = "Receptionist,Manager")]
[Route("api/customers")]
public class CustomersController : AuthenticatedControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var result = await _customerService.SearchAsync(new CustomerSearchRequest
        {
            ActorRole = CurrentRole()
        });

        return Ok(new ApiResponse<CustomerListResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchCustomers([FromQuery] string? keyword)
    {
        var result = await _customerService.SearchAsync(new CustomerSearchRequest
        {
            Keyword = keyword,
            ActorRole = CurrentRole()
        });

        return Ok(new ApiResponse<CustomerListResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var result = await _customerService.GetByIdAsync(id, CurrentRole());
        return Ok(new ApiResponse<CustomerResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _customerService.CreateAsync(request);

        return Ok(new ApiResponse<CustomerResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
    {
        request.CustomerId = id;
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _customerService.UpdateAsync(request);

        return Ok(new ApiResponse<CustomerResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateCustomerStatus(int id, [FromBody] UpdateCustomerStatusRequest request)
    {
        request.CustomerId = id;
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _customerService.UpdateStatusAsync(request);

        return Ok(new ApiResponse<CustomerResponse> { Success = true, Message = result.Message, Data = result });
    }
}
