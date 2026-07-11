using R_SS.BLL.DTOs.Account;

namespace R_SS.BLL.Interfaces;

public interface IAccountManagementService
{
    Task<AccountResponse> AddAsync(ManageAccountRequest request);
    Task<AccountResponse> UpdateAsync(ManageAccountRequest request);
    Task<AccountResponse> SetLockAsync(int userId, int actorUserId, string actorRole, bool isLocked);
    Task<AccountResponse> DeleteAsync(int userId, int actorUserId, string actorRole);
}
