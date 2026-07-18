using Microsoft.AspNetCore.Mvc;

namespace R_SS.Web.Controllers
{
    public class TechnicianController : Controller
    {
        public IActionResult TechnicianDashboard() => View();

        public IActionResult AssignedTasks() => View();

        public IActionResult TaskDetails() => View();

        public IActionResult RecordFaults() => View();

        public IActionResult ProposeRepairPlan() => View();

        public IActionResult UpdateProgress() => View();

        public IActionResult ComponentSearch() => View();

        public IActionResult UpdateComponents() => View();

        public IActionResult RecordResults() => View();

        public IActionResult WorkSchedule() => View();
    }
}
