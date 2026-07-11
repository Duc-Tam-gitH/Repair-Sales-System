using R_SS.BLL.DTOs.Promotion;

namespace R_SS.BLL.Interfaces;

public interface IPromotionService
{
    Task<PromotionResponse> AddAsync(ManagePromotionRequest request);
    Task<PromotionResponse> UpdateAsync(ManagePromotionRequest request);
    Task<PromotionResponse> DeleteAsync(int promotionId, int actorUserId, string actorRole);
}
