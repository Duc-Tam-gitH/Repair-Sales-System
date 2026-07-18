using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Inventory;
using R_SS.BLL.DTOs.Product;
using R_SS.BLL.DTOs.Supplier;
using R_SS.BLL.Interfaces;
using R_SS.Web.Models;

namespace R_SS.Web.Controllers
{
    [Authorize]
    public class WarehouseController : Controller
    {
        private readonly IProductService _productService;
        private readonly IInventoryService _inventoryService;
        private readonly ISupplierManagementService _supplierManagementService;

        public WarehouseController(
            IProductService productService,
            IInventoryService inventoryService,
            ISupplierManagementService supplierManagementService)
        {
            _productService = productService;
            _inventoryService = inventoryService;
            _supplierManagementService = supplierManagementService;
        }

        public async Task<IActionResult> WarehouseDashboard()
        {
            var products = await _productService.GetProductsAsync();
            var history = await GetHistoryAsync();
            var productList = products.Products;

            var model = new WarehouseDashboardViewModel
            {
                Products = productList,
                RecentTransactions = history.Transactions
                    .OrderByDescending(transaction => transaction.CreatedAtUtc)
                    .Take(5)
                    .ToArray(),
                TotalStockQuantity = productList.Sum(product => product.QuantityInStock),
                LowStockCount = productList.Count(IsLowStock),
                OutOfStockCount = productList.Count(product => product.QuantityInStock <= 0),
                EstimatedInventoryValue = productList.Sum(product => product.QuantityInStock * product.SalePrice)
            };

            return View(model);
        }

        public async Task<IActionResult> StockList(string? keyword = null)
        {
            var model = string.IsNullOrWhiteSpace(keyword)
                ? await _productService.GetProductsAsync()
                : await _productService.SearchAsync(new SearchProductsRequest { Keyword = keyword, Criteria = "all" });

            ViewData["Keyword"] = keyword;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> InventoryManagement()
        {
            return View(await BuildInventoryViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InventoryManagement(InventoryTransactionRequest transaction)
        {
            transaction.ActorUserId = GetActorUserId();
            transaction.ActorRole = RoleConstants.Manager;

            try
            {
                var result = await _inventoryService.ApplyTransactionAsync(transaction);
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(InventoryDetails), new { id = transaction.ProductId });
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(await BuildInventoryViewModelAsync(transaction));
            }
        }

        public async Task<IActionResult> InventoryDetails(int id)
        {
            try
            {
                var product = await _productService.GetProductDetailsAsync(id);
                var history = await GetHistoryAsync(id);

                return View(new WarehouseProductDetailsViewModel
                {
                    Product = product,
                    Transactions = history.Transactions
                        .OrderByDescending(transaction => transaction.CreatedAtUtc)
                        .ToArray()
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = GetErrorMessage(ex);
                return RedirectToAction(nameof(StockList));
            }
        }

        public async Task<IActionResult> InventoryTracking(int? productId = null, string? transactionType = null)
        {
            var history = await GetHistoryAsync(productId, transactionType);
            ViewData["ProductId"] = productId;
            ViewData["TransactionType"] = transactionType;
            return View(history);
        }

        public async Task<IActionResult> SupplierManagement(string? keyword = null)
        {
            return View(await BuildSupplierListViewModelAsync(keyword));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSupplier(ManageSupplierRequest supplier, string? keyword = null)
        {
            supplier.ActorUserId = GetActorUserId();
            supplier.ActorRole = RoleConstants.Manager;
            supplier.IsActive = true;

            try
            {
                var result = await _supplierManagementService.AddAsync(supplier);
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(SupplierManagement));
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(nameof(SupplierManagement), await BuildSupplierListViewModelAsync(keyword, supplier));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSupplier(ManageSupplierRequest supplier, string? keyword = null)
        {
            supplier.ActorUserId = GetActorUserId();
            supplier.ActorRole = RoleConstants.Manager;

            try
            {
                var result = await _supplierManagementService.UpdateAsync(supplier);
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(SupplierManagement));
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(nameof(SupplierManagement), await BuildSupplierListViewModelAsync(keyword, supplier));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableSupplier(int supplierId)
        {
            try
            {
                var result = await _supplierManagementService.DisableAsync(supplierId, GetActorUserId(), RoleConstants.Manager);
                TempData["SuccessMessage"] = result.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = GetErrorMessage(ex);
            }

            return RedirectToAction(nameof(SupplierManagement));
        }

        private async Task<WarehouseInventoryViewModel> BuildInventoryViewModelAsync(InventoryTransactionRequest? transaction = null)
        {
            var products = await _productService.GetProductsAsync();
            var history = await GetHistoryAsync();

            return new WarehouseInventoryViewModel
            {
                Products = products.Products.OrderBy(product => product.ProductName).ToArray(),
                Transactions = history.Transactions.OrderByDescending(item => item.CreatedAtUtc).Take(10).ToArray(),
                Transaction = transaction ?? new InventoryTransactionRequest { TransactionType = "Receipt" }
            };
        }

        private async Task<WarehouseSupplierListViewModel> BuildSupplierListViewModelAsync(string? keyword, ManageSupplierRequest? supplier = null)
        {
            var suppliers = await _supplierManagementService.GetSuppliersAsync(keyword);

            return new WarehouseSupplierListViewModel
            {
                Suppliers = suppliers.Suppliers,
                Supplier = supplier ?? new ManageSupplierRequest { IsActive = true },
                Keyword = keyword
            };
        }

        private Task<InventoryHistoryResponse> GetHistoryAsync(int? productId = null, string? transactionType = null)
        {
            return _inventoryService.GetHistoryAsync(new InventoryHistoryRequest
            {
                ActorUserId = GetActorUserId(),
                ActorRole = RoleConstants.Manager,
                ProductId = productId,
                TransactionType = string.IsNullOrWhiteSpace(transactionType) ? null : transactionType
            });
        }

        private int GetActorUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
        }

        private static bool IsLowStock(ProductResponse product)
        {
            return product.QuantityInStock > 0 && product.QuantityInStock <= 5;
        }

        private void AddError(Exception exception)
        {
            if (exception is ValidationException validationException)
            {
                foreach (var error in validationException.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return;
            }

            ModelState.AddModelError(string.Empty, exception.Message);
        }

        private static string GetErrorMessage(Exception exception)
        {
            return exception is ValidationException validationException
                ? string.Join(" ", validationException.Errors.Select(error => error.ErrorMessage))
                : exception.Message;
        }
    }
}
