using R_SS.BLL.DTOs.Supplier;

namespace R_SS.BLL.Interfaces;

public interface ISupplierManagementService
{
    Task<SupplierListResponse> GetSuppliersAsync(string? keyword = null);
    Task<SupplierResponse> AddAsync(ManageSupplierRequest request);
    Task<SupplierResponse> UpdateAsync(ManageSupplierRequest request);
    Task<SupplierResponse> DisableAsync(int supplierId, int actorUserId, string actorRole);
}
