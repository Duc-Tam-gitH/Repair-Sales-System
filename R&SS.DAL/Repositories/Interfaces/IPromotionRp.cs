using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IPromotionRp : IGenericRp<Promotion>
    {
        Task<Promotion?> GetWithProductsAsync(int promotionId);
        Task<bool> ExistsCodeAsync(string promotionCode, int? excludedPromotionId = null);
        Task<bool> HasActiveOrdersAsync(int promotionId);
    }
}
