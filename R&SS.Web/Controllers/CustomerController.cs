using Microsoft.AspNetCore.Mvc;

namespace R_SS.Web.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult CustomerHome() => View();

        public IActionResult ProductList() => View();

        public IActionResult ProductDetails() => View();

        public IActionResult Cart() => View();

        public IActionResult Checkout() => View();

        public IActionResult MyOrders() => View();

        public IActionResult OrderDetails() => View();

        public IActionResult SubmitRepair() => View();

        public IActionResult SearchRepair() => View();

        public IActionResult RepairProgress() => View();

        public IActionResult RepairHistory() => View();

        public IActionResult ServiceRating() => View();
    }
}
