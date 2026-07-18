namespace R_SS.BLL.DTOs.Supplier;

public class SupplierListResponse
{
    public IReadOnlyCollection<SupplierResponse> Suppliers { get; set; } = Array.Empty<SupplierResponse>();
    public string Message { get; set; } = string.Empty;
}
