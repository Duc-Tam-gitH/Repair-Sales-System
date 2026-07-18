using Microsoft.AspNetCore.Mvc;

namespace R_SS.Web.Controllers
{
    public class ProductManagementController : Controller
    {
        public IActionResult CategoryManagement() => View();
        public IActionResult ProductList() => View();
        public IActionResult AddUpdateProduct() => View();
        public IActionResult ProductDetails() => View();
        public IActionResult OrderList() => View();
    }
}
