using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Invoice;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Web.Models;

namespace R_SS.Web.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentController(IInvoiceService invoiceService, IUnitOfWork unitOfWork)
        {
            _invoiceService = invoiceService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> TransactionList(string? keyword = null, string? status = null)
        {
            var payments = await _unitOfWork.Payments.GetAllWithDetailsAsync();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                payments = payments.Where(payment =>
                    payment.PaymentCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (payment.Customer?.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)).ToArray();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                payments = payments.Where(payment => payment.PaymentStatus.Equals(status, StringComparison.OrdinalIgnoreCase)).ToArray();
            }

            return View(new PaymentListViewModel { Payments = payments, Keyword = keyword, Status = status });
        }

        public async Task<IActionResult> TransactionDetails(int id)
        {
            var payment = await _unitOfWork.Payments.GetWithDetailsAsync(id);
            if (payment is null)
            {
                TempData["ErrorMessage"] = "Payment transaction not found.";
                return RedirectToAction(nameof(TransactionList));
            }

            return View(payment);
        }

        public async Task<IActionResult> RepairPayment()
        {
            var repairs = await _unitOfWork.RepairOrders.GetVisibleTicketsAsync(RoleConstants.Manager, GetActorUserId(), null);
            return View(new RepairPaymentViewModel
            {
                CompletedRepairs = repairs
                    .Where(repair => repair.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(repair => repair.CompletedDate ?? repair.UpdatedAt)
                    .ToArray()
            });
        }

        public async Task<IActionResult> InvoicePrint(int paymentId, bool exportPdf = false, bool sendEmail = false)
        {
            var payment = await _unitOfWork.Payments.GetWithDetailsAsync(paymentId);
            if (payment is null)
            {
                TempData["ErrorMessage"] = "Payment transaction not found.";
                return RedirectToAction(nameof(TransactionList));
            }

            InvoiceResponse? invoice = null;
            try
            {
                var transactionType = payment.SalesOrderId.HasValue ? "Sales" : "Repair";
                var transactionId = payment.SalesOrderId ?? payment.RepairOrderId ?? 0;
                invoice = await _invoiceService.PrintAsync(new PrintInvoiceRequest
                {
                    TransactionId = transactionId,
                    TransactionType = transactionType,
                    ActorUserId = GetActorUserId(),
                    ActorRole = RoleConstants.Manager,
                    ExportPdf = exportPdf,
                    SendEmail = sendEmail
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return View(new PaymentInvoiceViewModel { Payment = payment, Invoice = invoice });
        }

        private int GetActorUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
        }
    }
}
