using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces
{
    public interface ICartRp : IGenericRp<Cart>
    {
        Task<Cart?> GetByCustomerIdAsync(int customerId);
    }
}
