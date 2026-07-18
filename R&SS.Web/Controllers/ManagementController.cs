using Microsoft.AspNetCore.Mvc;

namespace R_SS.Web.Controllers
{
    public class ManagementController : Controller
    {
        public IActionResult ManagementDashboard() => View();
        public IActionResult RepairProgressDashboard() => View();
        public IActionResult SystemAccountManagement() => View();
        public IActionResult AddAccount() => View();
        public IActionResult RoleAuthorization() => View();
        public IActionResult EmployeeManagement() => View();
        public IActionResult CustomerManagement() => View();
        public IActionResult TechnicalRequestManagement() => View();
        public IActionResult ApproveQuotes() => View();
    }
}
