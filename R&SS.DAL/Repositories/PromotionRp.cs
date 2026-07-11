using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class PromotionRp : GenericRp<Promotion>, IPromotionRp
    {
        public PromotionRp(AppDbContext context) : base(context)
        {
        }

        public async Task<Promotion?> GetWithProductsAsync(int promotionId)
        {
            return await _context.Promotions
                .Include(promotion => promotion.PromotionProducts)
                .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(promotion => promotion.PromotionId == promotionId);
        }

        public async Task<bool> ExistsCodeAsync(string promotionCode, int? excludedPromotionId = null)
        {
            return await _context.Promotions.AnyAsync(promotion =>
                promotion.PromotionCode.ToLower() == promotionCode.ToLower() &&
                (!excludedPromotionId.HasValue || promotion.PromotionId != excludedPromotionId.Value));
        }

        public Task<bool> HasActiveOrdersAsync(int promotionId)
        {
            return Task.FromResult(false);
        }
    }
}
