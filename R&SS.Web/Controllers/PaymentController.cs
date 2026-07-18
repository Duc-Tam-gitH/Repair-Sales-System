using Microsoft.AspNetCore.Mvc;

namespace R_SS.Web.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult TransactionList() => View();
        public IActionResult TransactionDetails() => View();
        public IActionResult RepairPayment() => View();
        public IActionResult InvoicePrint() => View();
    }
}
