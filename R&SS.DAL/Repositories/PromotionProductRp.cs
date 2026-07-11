using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class PromotionProductRp : GenericRp<PromotionProduct>, IPromotionProductRp
    {
        public PromotionProductRp(AppDbContext context) : base(context)
        {
        }

        public void DeleteRange(IEnumerable<PromotionProduct> items)
        {
            _context.PromotionProducts.RemoveRange(items);
        }
    }
}
