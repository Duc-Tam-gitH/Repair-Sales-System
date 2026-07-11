using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories.Interfaces;

public interface IPasswordResetRequestRp : IGenericRp<PasswordResetRequest>
{
    Task<PasswordResetRequest?> GetByUserIdAsync(int userId);
}
