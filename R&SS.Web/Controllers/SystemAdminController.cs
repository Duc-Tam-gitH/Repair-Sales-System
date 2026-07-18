using Microsoft.AspNetCore.Mvc;

namespace R_SS.Web.Controllers
{
    public class SystemAdminController : Controller
    {
        public IActionResult AuditLog() => View();
        public IActionResult AuditLogDetails() => View();
        public IActionResult ProcessManagement() => View();
    }
}
