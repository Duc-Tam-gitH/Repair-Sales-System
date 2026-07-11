namespace R_SS.Models.Entities
{
    public class PromotionProduct
    {
        public int PromotionProductId { get; set; }
        public int PromotionId { get; set; }
        public int ProductId { get; set; }
        public virtual Promotion? Promotion { get; set; }
        public virtual Product? Product { get; set; }
    }
}
