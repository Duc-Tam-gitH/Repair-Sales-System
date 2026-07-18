using Microsoft.AspNetCore.Mvc;

namespace R_SS.Web.Controllers
{
    public class WarehouseController : Controller
    {
        public IActionResult WarehouseDashboard() => View();
        public IActionResult StockList() => View();
        public IActionResult InventoryManagement() => View();
        public IActionResult InventoryDetails() => View();
        public IActionResult InventoryTracking() => View();
        public IActionResult SupplierManagement() => View();
    }
}
