using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Authentication;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public class ProfileController : AuthenticatedControllerBase
{
    private readonly IAuthService _authService;

    public ProfileController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _authService.GetPersonalInfoAsync(CurrentUserId());
        return Ok(new ApiResponse<PersonalInfoResponse>
        {
            Success = true,
            Message = "Personal information retrieved successfully.",
            Data = result
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatePersonalInfoRequest request)
    {
        request.UserId = CurrentUserId();
        var result = await _authService.UpdatePersonalInfoAsync(request);

        return Ok(new ApiResponse<PersonalInfoResponse>
        {
            Success = true,
            Message = "Personal information updated successfully.",
            Data = result
        });
    }
}
