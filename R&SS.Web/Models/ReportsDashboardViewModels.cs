using R_SS.BLL.DTOs.Inventory;
using R_SS.BLL.DTOs.Report;
using R_SS.Models.Entities;

namespace R_SS.Web.Models;

public class RevenueReportViewModel
{
    public RevenueReportRequest Request { get; set; } = new();
    public RevenueReportResponse Report { get; set; } = new();
}

public class InventoryReportViewModel
{
    public InventoryStatisticResponse Report { get; set; } = new();
    public decimal EstimatedValue { get; set; }
}

public class RepairReportViewModel
{
    public IReadOnlyCollection<RepairOrder> Repairs { get; set; } = Array.Empty<RepairOrder>();
}

public class CustomerStatisticsViewModel
{
    public IReadOnlyCollection<Customer> Customers { get; set; } = Array.Empty<Customer>();
    public IReadOnlyCollection<SalesOrder> Orders { get; set; } = Array.Empty<SalesOrder>();
    public IReadOnlyCollection<RepairOrder> Repairs { get; set; } = Array.Empty<RepairOrder>();
}

public class ProductSalesStatisticsViewModel
{
    public IReadOnlyCollection<SalesOrderDetail> Details { get; set; } = Array.Empty<SalesOrderDetail>();
}
