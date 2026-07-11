namespace R_SS.BLL.DTOs.ProductCategory;

public class ManageProductCategoryRequest
{
    public int? ProductCategoryId { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
