using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Inventory;
using R_SS.BLL.DTOs.Report;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Web.Models;

namespace R_SS.Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IRevenueReportService _revenueReportService;
        private readonly IInventoryStatisticService _inventoryStatisticService;
        private readonly IUnitOfWork _unitOfWork;

        public ReportsController(
            IRevenueReportService revenueReportService,
            IInventoryStatisticService inventoryStatisticService,
            IUnitOfWork unitOfWork)
        {
            _revenueReportService = revenueReportService;
            _inventoryStatisticService = inventoryStatisticService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> RevenueReport(DateTime? fromUtc = null, DateTime? toUtc = null, string revenueType = "all", string exportFormat = "none")
        {
            var request = new RevenueReportRequest
            {
                ActorRole = RoleConstants.Manager,
                FromUtc = fromUtc ?? DateTime.UtcNow.Date.AddDays(-30),
                ToUtc = toUtc ?? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1),
                RevenueType = revenueType,
                ExportFormat = exportFormat
            };

            return View(new RevenueReportViewModel
            {
                Request = request,
                Report = await _revenueReportService.GenerateAsync(request)
            });
        }

        public async Task<IActionResult> RepairReport()
        {
            var repairs = await _unitOfWork.RepairOrders.GetVisibleTicketsAsync(RoleConstants.Manager, 0, null);
            return View(new RepairReportViewModel { Repairs = repairs });
        }

        public async Task<IActionResult> InventoryReport(string stockStatus = "all")
        {
            var request = new InventoryStatisticRequest
            {
                ActorRole = RoleConstants.Manager,
                StockStatus = stockStatus
            };
            var report = await _inventoryStatisticService.GenerateAsync(request);
            var products = await _unitOfWork.Products.GetAllAsync();

            return View(new InventoryReportViewModel
            {
                Report = report,
                EstimatedValue = products.Sum(product => product.QuantityInStock * product.SalePrice)
            });
        }

        public async Task<IActionResult> CustomerStatistics()
        {
            return View(new CustomerStatisticsViewModel
            {
                Customers = (await _unitOfWork.Customers.GetAllAsync()).ToArray(),
                Orders = await _unitOfWork.SalesOrders.GetAllWithDetailsAsync(),
                Repairs = await _unitOfWork.RepairOrders.GetVisibleTicketsAsync(RoleConstants.Manager, 0, null)
            });
        }

        public async Task<IActionResult> ProductSalesStatistics()
        {
            var orders = await _unitOfWork.SalesOrders.GetAllWithDetailsAsync();
            var details = orders
                .Where(order => !order.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                .SelectMany(order => order.SalesOrderDetails)
                .ToArray();

            return View(new ProductSalesStatisticsViewModel { Details = details });
        }
    }
}
