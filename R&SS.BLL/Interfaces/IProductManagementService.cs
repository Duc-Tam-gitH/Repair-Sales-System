using R_SS.BLL.DTOs.Product;

namespace R_SS.BLL.Interfaces;

public interface IProductManagementService
{
    Task<ProductManagementResponse> AddAsync(ManageProductRequest request);
    Task<ProductManagementResponse> UpdateAsync(ManageProductRequest request);
    Task<ProductManagementResponse> DeleteAsync(int productId, int actorUserId, string actorRole);
}
