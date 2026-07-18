using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.BLL.DTOs.Cart;
using R_SS.BLL.DTOs.Feedback;
using R_SS.BLL.DTOs.Order;
using R_SS.BLL.DTOs.Product;
using R_SS.BLL.DTOs.ServiceRequest;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;
using R_SS.Web.Models;

namespace R_SS.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IFeedbackService _feedbackService;
        private readonly IUnitOfWork _unitOfWork;

        public CustomerController(
            IProductService productService,
            ICartService cartService,
            IOrderService orderService,
            IServiceRequestService serviceRequestService,
            IFeedbackService feedbackService,
            IUnitOfWork unitOfWork)
        {
            _productService = productService;
            _cartService = cartService;
            _orderService = orderService;
            _serviceRequestService = serviceRequestService;
            _feedbackService = feedbackService;
            _unitOfWork = unitOfWork;
        }

        public IActionResult CustomerHome() => View();

        public async Task<IActionResult> ProductList(string? keyword = null, string criteria = "all")
        {
            try
            {
                var model = string.IsNullOrWhiteSpace(keyword)
                    ? await _productService.GetProductsAsync()
                    : await _productService.SearchAsync(new SearchProductsRequest
                    {
                        Keyword = keyword,
                        Criteria = string.IsNullOrWhiteSpace(criteria) ? "all" : criteria
                    });

                ViewData["Keyword"] = keyword;
                ViewData["Criteria"] = criteria;
                return View(model);
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(new ProductListResponse());
            }
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            try
            {
                var model = await _productService.GetProductDetailsAsync(id);
                return View(model);
            }
            catch (Exception ex)
            {
                AddError(ex);
                return RedirectToAction(nameof(ProductList));
            }
        }

        [Authorize]
        public async Task<IActionResult> Cart()
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                var model = await _cartService.GetCartAsync(customerId);
                return View(model);
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(new CartResponse());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddToCart(AddToCartRequest request)
        {
            try
            {
                request.CustomerId = await GetCurrentCustomerIdAsync();
                await _cartService.AddToCartAsync(request);
                TempData["SuccessMessage"] = "Product added to cart successfully.";
                return RedirectToAction(nameof(Cart));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = GetErrorMessage(ex);
                return RedirectToAction(nameof(ProductDetails), new { id = request.ProductId });
            }
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var customer = await GetCurrentCustomerAsync();
                var cart = await _cartService.GetCartAsync(customer.CustomerId);
                return View(new CustomerCheckoutViewModel
                {
                    Cart = cart,
                    Order = new PlaceOrderRequest
                    {
                        CustomerId = customer.CustomerId,
                        RecipientName = customer.FullName,
                        DeliveryAddress = customer.Address ?? string.Empty,
                        PaymentMethod = "Cash"
                    }
                });
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(new CustomerCheckoutViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Checkout(PlaceOrderRequest order)
        {
            try
            {
                order.CustomerId = await GetCurrentCustomerIdAsync();
                var response = await _orderService.PlaceOrderAsync(order);
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction(nameof(OrderDetails), new { id = response.SalesOrderId });
            }
            catch (Exception ex)
            {
                AddError(ex);
                var cart = await _cartService.GetCartAsync(order.CustomerId);
                return View(new CustomerCheckoutViewModel { Cart = cart, Order = order });
            }
        }

        [Authorize]
        public async Task<IActionResult> MyOrders(string? keyword = null)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            var orders = await _unitOfWork.SalesOrders.GetByCustomerIdAsync(customerId);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                orders = orders
                    .Where(order => order.SalesOrderCode.Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }

            ViewData["Keyword"] = keyword;
            return View(orders.Select(MapOrder).ToArray());
        }

        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            var order = await _unitOfWork.SalesOrders.GetWithDetailsAsync(id);
            if (order is null || order.CustomerId != customerId)
            {
                return RedirectToAction(nameof(MyOrders));
            }

            return View(MapOrder(order));
        }

        [Authorize]
        public IActionResult SubmitRepair()
        {
            return View(new SendServiceRequestRequest { ServiceType = "Repair" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> SubmitRepair(SendServiceRequestRequest request)
        {
            request.CustomerId = await GetCurrentCustomerIdAsync();
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                var response = await _serviceRequestService.SendAsync(request);
                TempData["SuccessMessage"] = $"{response.Message} Request Code: {response.RequestCode}";
                return RedirectToAction(nameof(SearchRepair), new { keyword = response.RequestCode });
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(request);
            }
        }

        [Authorize]
        public async Task<IActionResult> SearchRepair(string? keyword = null)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            var requests = await _serviceRequestService.GetByCustomerAsync(customerId);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                requests = requests
                    .Where(request => request.RequestCode.Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }

            ViewData["Keyword"] = keyword;
            return View(requests);
        }

        [Authorize]
        public async Task<IActionResult> RepairProgress()
        {
            var customerId = await GetCurrentCustomerIdAsync();
            var repairs = await _unitOfWork.RepairOrders.GetSubmittedByCustomerIdAsync(customerId, includeCanceled: false);
            return View(repairs.Where(repair => !IsClosedRepair(repair.Status)).ToArray());
        }

        [Authorize]
        public async Task<IActionResult> RepairHistory()
        {
            var customerId = await GetCurrentCustomerIdAsync();
            var repairs = await _unitOfWork.RepairOrders.GetSubmittedByCustomerIdAsync(customerId, includeCanceled: true);
            return View(repairs.Where(repair => IsClosedRepair(repair.Status)).ToArray());
        }

        [Authorize]
        public async Task<IActionResult> ServiceRating()
        {
            var customerId = await GetCurrentCustomerIdAsync();
            return View(new ServiceRatingViewModel
            {
                EligibleItems = await _feedbackService.GetEligibleItemsAsync(customerId),
                Feedback = new SubmitFeedbackRequest { CustomerId = customerId, Rating = 5 }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ServiceRating(SubmitFeedbackRequest feedback)
        {
            feedback.CustomerId = await GetCurrentCustomerIdAsync();
            try
            {
                var response = await _feedbackService.SubmitAsync(feedback);
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction(nameof(ServiceRating));
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(new ServiceRatingViewModel
                {
                    EligibleItems = await _feedbackService.GetEligibleItemsAsync(feedback.CustomerId),
                    Feedback = feedback
                });
            }
        }

        private async Task<int> GetCurrentCustomerIdAsync()
        {
            return (await GetCurrentCustomerAsync()).CustomerId;
        }

        private async Task<Customer> GetCurrentCustomerAsync()
        {
            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var value)
                ? value
                : 0;

            if (userId <= 0)
            {
                throw new UnauthorizedAccessException("Please log in as a customer.");
            }

            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer is null)
            {
                throw new InvalidOperationException("Customer profile was not found for this account.");
            }

            return customer;
        }

        private void AddError(Exception exception)
        {
            ModelState.AddModelError(string.Empty, GetErrorMessage(exception));
        }

        private static string GetErrorMessage(Exception exception)
        {
            if (exception is ValidationException validationException)
            {
                return string.Join(" ", validationException.Errors.Select(error => error.ErrorMessage));
            }

            return exception.Message;
        }

        private static OrderResponse MapOrder(SalesOrder order)
        {
            var payment = order.Payments.FirstOrDefault();
            return new OrderResponse
            {
                SalesOrderId = order.SalesOrderId,
                SalesOrderCode = order.SalesOrderCode,
                CustomerId = order.CustomerId,
                Status = order.Status,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount,
                PaymentMethod = payment?.PaymentMethod ?? string.Empty,
                Items = order.SalesOrderDetails.Select(detail => new OrderItemResponse
                {
                    ProductId = detail.ProductId,
                    ProductName = detail.Product?.ProductName ?? string.Empty,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    DiscountAmount = detail.DiscountAmount,
                    LineTotal = detail.LineTotal
                }).ToArray(),
                Message = order.Notes ?? string.Empty
            };
        }

        private static bool IsClosedRepair(string status)
        {
            return status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Delivered", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Canceled", StringComparison.OrdinalIgnoreCase);
        }
    }
}
