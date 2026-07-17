using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Promotion;
using R_SS.BLL.Interfaces;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize(Roles = "Manager")]
[Route("api/promotions")]
public class PromotionsController : AuthenticatedControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionsController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePromotion([FromBody] ManagePromotionRequest request)
    {
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _promotionService.AddAsync(request);
        return Ok(new ApiResponse<PromotionResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePromotion(int id, [FromBody] ManagePromotionRequest request)
    {
        request.PromotionId = id;
        request.ActorRole = CurrentRole();
        request.ActorUserId = CurrentUserId();
        var result = await _promotionService.UpdateAsync(request);
        return Ok(new ApiResponse<PromotionResponse> { Success = true, Message = result.Message, Data = result });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePromotion(int id)
    {
        var result = await _promotionService.DeleteAsync(id, CurrentUserId(), CurrentRole());
        return Ok(new ApiResponse<PromotionResponse> { Success = true, Message = result.Message, Data = result });
    }
}
