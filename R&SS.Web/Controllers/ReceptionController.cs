using Microsoft.AspNetCore.Mvc;

namespace R_SS.Web.Controllers
{
    public class ReceptionController : Controller
    {
        public IActionResult ReceptionDashboard() => View();

        public IActionResult CustomerList() => View();

        public IActionResult AddCustomer() => View();

        public IActionResult CustomerDetails() => View();

        public IActionResult RepairRequestList() => View();

        public IActionResult EquipmentReception() => View();

        public IActionResult CreateTechnicalReport() => View();

        public IActionResult TechnicalReportList() => View();

        public IActionResult TechnicalReportDetails() => View();

        public IActionResult DeviceHandover() => View();
    }
}
