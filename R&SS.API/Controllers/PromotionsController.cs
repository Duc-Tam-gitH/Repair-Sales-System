using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.API.Responses;
using R_SS.BLL.DTOs.Promotion;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;

namespace R_SS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/promotions")]
public class PromotionsController : AuthenticatedControllerBase
{
    private readonly IPromotionService _promotionService;
    private readonly IUnitOfWork _unitOfWork;

    public PromotionsController(IPromotionService promotionService, IUnitOfWork unitOfWork)
    {
        _promotionService = promotionService;
        _unitOfWork = unitOfWork;
    }

    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<IActionResult> GetActivePromotions()
    {
        var now = DateTime.UtcNow;
        var promotions = await _unitOfWork.Promotions.GetAllAsync();
        var data = promotions
            .Where(promotion => promotion.IsActive && promotion.StartDateUtc <= now && promotion.EndDateUtc >= now)
            .Select(MapPromotion)
            .ToArray();

        return Ok(new ApiResponse<IReadOnlyCollection<PromotionResponse>>
        {
            Success = true,
            Message = data.Length == 0 ? "No active promotions found." : "Active promotions retrieved successfully.",
            Data = data
        });
    }

    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<IActionResult> ValidatePromotion([FromBody] ValidatePromotionRequest request)
    {
        var promotions = await _unitOfWork.Promotions.GetAllAsync();
        var promotion = promotions.FirstOrDefault(item =>
            item.PromotionCode.Equals(request.PromotionCode, StringComparison.OrdinalIgnoreCase));
        var now = DateTime.UtcNow;
        var isValid = promotion is not null && promotion.IsActive && promotion.StartDateUtc <= now && promotion.EndDateUtc >= now;
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = isValid ? "Promotion is valid." : "Promotion is invalid or expired.",
            Data = new { isValid, promotion = promotion is null ? null : MapPromotion(promotion) }
        });
    }

    [Authorize(Roles = "Manager")]
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
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> DeletePromotion(int id)
    {
        var result = await _promotionService.DeleteAsync(id, CurrentUserId(), CurrentRole());
        return Ok(new ApiResponse<PromotionResponse> { Success = true, Message = result.Message, Data = result });
    }

    private static PromotionResponse MapPromotion(R_SS.Models.Entities.Promotion promotion)
    {
        return new PromotionResponse
        {
            PromotionId = promotion.PromotionId,
            PromotionCode = promotion.PromotionCode,
            ProgramName = promotion.ProgramName,
            PromotionType = promotion.PromotionType,
            PromotionValue = promotion.PromotionValue,
            StartDateUtc = promotion.StartDateUtc,
            EndDateUtc = promotion.EndDateUtc,
            IsActive = promotion.IsActive,
            ProductIds = promotion.PromotionProducts.Select(product => product.ProductId).ToArray(),
            Message = string.Empty
        };
    }

    public sealed class ValidatePromotionRequest
    {
        public string PromotionCode { get; set; } = string.Empty;
    }
}
