using FluentValidation;
using FluentValidation.Results;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Inventory;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<InventoryTransactionRequest> _transactionValidator;
    private readonly IValidator<InventoryHistoryRequest> _historyValidator;

    public InventoryService(IUnitOfWork unitOfWork, IValidator<InventoryTransactionRequest> transactionValidator, IValidator<InventoryHistoryRequest> historyValidator)
    {
        _unitOfWork = unitOfWork;
        _transactionValidator = transactionValidator;
        _historyValidator = historyValidator;
    }

    public async Task<InventoryTransactionResponse> ApplyTransactionAsync(InventoryTransactionRequest request)
    {
        ThrowIfInvalid(await _transactionValidator.ValidateAsync(request));
        EnsureManager(request.ActorRole);
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product is null) throw new NotFoundException("Product not found.");
        var before = product.QuantityInStock;
        var change = request.TransactionType switch
        {
            "Receipt" => request.Quantity,
            "Issue" => -request.Quantity,
            "Adjustment" => request.Quantity - before,
            _ => 0
        };
        if (before + change < 0) throw new InvalidOperationException("Stock is insufficient.");
        product.QuantityInStock = before + change;
        product.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Products.Update(product);
        var transaction = new InventoryTransaction
        {
            Product = product,
            ProductId = product.ProductId,
            ActorUserId = request.ActorUserId,
            TransactionType = request.TransactionType,
            QuantityChange = change,
            StockBefore = before,
            StockAfter = product.QuantityInStock,
            Reason = Normalize(request.Reason),
            CreatedAtUtc = DateTime.UtcNow
        };
        await _unitOfWork.InventoryTransactions.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();
        return Map(transaction, "Inventory updated successfully.");
    }

    public async Task<InventoryHistoryResponse> GetHistoryAsync(InventoryHistoryRequest request)
    {
        ThrowIfInvalid(await _historyValidator.ValidateAsync(request));
        EnsureManager(request.ActorRole);
        var transactions = await _unitOfWork.InventoryTransactions.SearchAsync(request.ProductId, request.FromUtc, request.ToUtc, request.TransactionType);
        var mapped = transactions.Select(transaction => Map(transaction, string.Empty)).ToArray();
        return new InventoryHistoryResponse { Transactions = mapped, Message = mapped.Length == 0 ? "No inventory history found." : "Inventory history retrieved successfully." };
    }

    private static InventoryTransactionResponse Map(InventoryTransaction transaction, string message) => new()
    {
        InventoryTransactionId = transaction.InventoryTransactionId,
        ProductId = transaction.ProductId,
        ProductName = transaction.Product?.ProductName ?? string.Empty,
        TransactionType = transaction.TransactionType,
        QuantityChange = transaction.QuantityChange,
        StockBefore = transaction.StockBefore,
        StockAfter = transaction.StockAfter,
        Reason = transaction.Reason,
        CreatedAtUtc = transaction.CreatedAtUtc,
        Message = message
    };

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void EnsureManager(string role)
    {
        if (!role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedException("Only Managers can manage inventory.");
    }
    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid) throw new ValidationException(result.Errors);
    }
}
