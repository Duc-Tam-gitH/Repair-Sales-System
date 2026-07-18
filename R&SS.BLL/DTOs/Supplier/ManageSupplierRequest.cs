namespace R_SS.BLL.DTOs.Supplier;

public class ManageSupplierRequest
{
    public int? SupplierId { get; set; }
    public int ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
