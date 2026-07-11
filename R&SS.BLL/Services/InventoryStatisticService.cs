using FluentValidation;
using FluentValidation.Results;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Inventory;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;

namespace R_SS.BLL.Services;

public class InventoryStatisticService : IInventoryStatisticService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<InventoryStatisticRequest> _validator;

    public InventoryStatisticService(IUnitOfWork unitOfWork, IValidator<InventoryStatisticRequest> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<InventoryStatisticResponse> GenerateAsync(InventoryStatisticRequest request)
    {
        ThrowIfInvalid(await _validator.ValidateAsync(request));
        EnsureManager(request.ActorRole);
        var products = (await _unitOfWork.Products.GetAllAsync()).Where(product =>
            !request.ProductCategoryId.HasValue || product.ProductCategoryId == request.ProductCategoryId.Value).ToArray();
        var transactions = await _unitOfWork.InventoryTransactions.SearchAsync(null, request.FromUtc, request.ToUtc, null);
        var items = products.Select(product =>
        {
            var productTransactions = transactions.Where(transaction => transaction.ProductId == product.ProductId).ToArray();
            var receipt = productTransactions.Where(transaction => transaction.QuantityChange > 0).Sum(transaction => transaction.QuantityChange);
            var issue = Math.Abs(productTransactions.Where(transaction => transaction.QuantityChange < 0).Sum(transaction => transaction.QuantityChange));
            var status = product.QuantityInStock <= product.ReorderLevel ? "Low Stock" : "Normal";
            return new InventoryStatisticItemResponse
            {
                ProductId = product.ProductId,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                ReceiptQuantity = receipt,
                IssueQuantity = issue,
                StockQuantity = product.QuantityInStock,
                InventoryStatus = status
            };
        }).Where(item => !request.StockStatus.Equals("low", StringComparison.OrdinalIgnoreCase) || item.InventoryStatus == "Low Stock").ToArray();
        return new InventoryStatisticResponse { Items = items, Message = items.Length == 0 ? "No statistical data." : "Inventory statistics generated successfully." };
    }

    private static void EnsureManager(string role)
    {
        if (!role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedException("Only Managers can view inventory statistics.");
    }
    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid) throw new ValidationException(result.Errors);
    }
}
