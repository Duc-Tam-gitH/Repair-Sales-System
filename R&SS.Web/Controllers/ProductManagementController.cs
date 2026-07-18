using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Order;
using R_SS.BLL.DTOs.Product;
using R_SS.BLL.DTOs.ProductCategory;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Web.Models;

namespace R_SS.Web.Controllers
{
    [Authorize]
    public class ProductManagementController : Controller
    {
        private readonly IProductService _productService;
        private readonly IProductManagementService _productManagementService;
        private readonly IProductCategoryManagementService _categoryManagementService;
        private readonly ISupplierManagementService _supplierManagementService;
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _unitOfWork;

        public ProductManagementController(
            IProductService productService,
            IProductManagementService productManagementService,
            IProductCategoryManagementService categoryManagementService,
            ISupplierManagementService supplierManagementService,
            IOrderService orderService,
            IUnitOfWork unitOfWork)
        {
            _productService = productService;
            _productManagementService = productManagementService;
            _categoryManagementService = categoryManagementService;
            _supplierManagementService = supplierManagementService;
            _orderService = orderService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> CategoryManagement(string? keyword = null)
        {
            return View(await BuildCategoryViewModelAsync(keyword));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(ManageProductCategoryRequest category, string? keyword = null)
        {
            category.ActorUserId = GetActorUserId();
            category.ActorRole = RoleConstants.Manager;
            category.IsActive = true;

            try
            {
                var result = await _categoryManagementService.AddAsync(category);
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(CategoryManagement));
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(nameof(CategoryManagement), await BuildCategoryViewModelAsync(keyword, category));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(ManageProductCategoryRequest category, string? keyword = null)
        {
            category.ActorUserId = GetActorUserId();
            category.ActorRole = RoleConstants.Manager;

            try
            {
                var result = await _categoryManagementService.UpdateAsync(category);
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(CategoryManagement));
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(nameof(CategoryManagement), await BuildCategoryViewModelAsync(keyword, category));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                var result = await _categoryManagementService.DeleteAsync(categoryId, GetActorUserId(), RoleConstants.Manager);
                TempData["SuccessMessage"] = result.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = GetErrorMessage(ex);
            }

            return RedirectToAction(nameof(CategoryManagement));
        }

        public async Task<IActionResult> ProductList(string? keyword = null)
        {
            var model = string.IsNullOrWhiteSpace(keyword)
                ? await _productService.GetProductsAsync()
                : await _productService.SearchAsync(new SearchProductsRequest { Keyword = keyword, Criteria = "all" });

            return View(new ProductCatalogViewModel
            {
                Products = model.Products,
                Keyword = keyword
            });
        }

        [HttpGet]
        public async Task<IActionResult> AddUpdateProduct(int? id = null)
        {
            return View(await BuildProductCatalogViewModelAsync(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUpdateProduct(ManageProductRequest product)
        {
            product.ActorUserId = GetActorUserId();
            product.ActorRole = RoleConstants.Manager;

            try
            {
                var result = product.ProductId.HasValue
                    ? await _productManagementService.UpdateAsync(product)
                    : await _productManagementService.AddAsync(product);

                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(ProductDetails), new { id = result.ProductId });
            }
            catch (Exception ex)
            {
                AddError(ex);
                return View(await BuildProductCatalogViewModelAsync(null, product));
            }
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            try
            {
                return View(await _productService.GetProductDetailsAsync(id));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = GetErrorMessage(ex);
                return RedirectToAction(nameof(ProductList));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            try
            {
                var result = await _productManagementService.DeleteAsync(productId, GetActorUserId(), RoleConstants.Manager);
                TempData["SuccessMessage"] = result.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = GetErrorMessage(ex);
            }

            return RedirectToAction(nameof(ProductList));
        }

        public async Task<IActionResult> OrderList(string? keyword = null, string? status = null)
        {
            var orders = await _unitOfWork.SalesOrders.GetAllWithDetailsAsync();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                orders = orders.Where(order =>
                    order.SalesOrderCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (order.Customer?.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)).ToArray();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(order => order.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToArray();
            }

            return View(new ProductOrderListViewModel
            {
                Orders = orders,
                Keyword = keyword,
                Status = status
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int salesOrderId, string reason = "Cancelled by product management.")
        {
            try
            {
                var result = await _orderService.CancelOrderAsync(new CancelOrderRequest
                {
                    SalesOrderId = salesOrderId,
                    ActorUserId = GetActorUserId(),
                    ActorRole = RoleConstants.Manager,
                    Reason = reason
                });
                TempData["SuccessMessage"] = result.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = GetErrorMessage(ex);
            }

            return RedirectToAction(nameof(OrderList));
        }

        private async Task<ProductCategoryManagementViewModel> BuildCategoryViewModelAsync(string? keyword, ManageProductCategoryRequest? category = null)
        {
            var categories = await _unitOfWork.ProductCategories.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                categories = categories.Where(item => item.CategoryName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            return new ProductCategoryManagementViewModel
            {
                Categories = categories.OrderBy(item => item.CategoryName).ToArray(),
                Category = category ?? new ManageProductCategoryRequest { IsActive = true },
                Keyword = keyword
            };
        }

        private async Task<ProductCatalogViewModel> BuildProductCatalogViewModelAsync(int? productId, ManageProductRequest? product = null)
        {
            var categories = await _unitOfWork.ProductCategories.GetAllAsync();
            var suppliers = await _supplierManagementService.GetSuppliersAsync();
            var request = product ?? new ManageProductRequest();

            if (productId.HasValue && product is null)
            {
                var detail = await _productService.GetProductDetailsAsync(productId.Value);
                var productEntity = await _unitOfWork.Products.GetByIdAsync(productId.Value);
                request = new ManageProductRequest
                {
                    ProductId = detail.ProductId,
                    ProductCode = detail.ProductCode,
                    ProductName = detail.ProductName,
                    ProductCategoryId = productEntity?.ProductCategoryId ?? 0,
                    SupplierId = productEntity?.SupplierId,
                    SalePrice = detail.SalePrice,
                    QuantityInStock = detail.QuantityInStock,
                    Description = detail.Description
                };
            }

            return new ProductCatalogViewModel
            {
                Categories = categories.Where(category => category.IsActive).OrderBy(category => category.CategoryName).ToArray(),
                Suppliers = suppliers.Suppliers.Where(supplier => supplier.IsActive).ToArray(),
                Product = request
            };
        }

        private int GetActorUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
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
