using R_SS.BLL.DTOs.ProductCategory;

namespace R_SS.BLL.Interfaces;

public interface IProductCategoryManagementService
{
    Task<ProductCategoryResponse> AddAsync(ManageProductCategoryRequest request);
    Task<ProductCategoryResponse> UpdateAsync(ManageProductCategoryRequest request);
    Task<ProductCategoryResponse> DeleteAsync(int categoryId, int actorUserId, string actorRole);
}
