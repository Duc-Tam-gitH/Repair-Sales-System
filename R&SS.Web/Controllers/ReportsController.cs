using Microsoft.AspNetCore.Mvc;

namespace R_SS.Web.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult RevenueReport() => View();
        public IActionResult RepairReport() => View();
        public IActionResult InventoryReport() => View();
        public IActionResult CustomerStatistics() => View();
        public IActionResult ProductSalesStatistics() => View();
    }
}
