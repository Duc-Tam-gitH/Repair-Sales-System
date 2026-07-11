using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface IPromotionProductRp : IGenericRp<PromotionProduct>
    {
        void DeleteRange(IEnumerable<PromotionProduct> items);
    }
}
