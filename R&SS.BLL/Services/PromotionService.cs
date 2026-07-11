using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Promotion;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class PromotionService : IPromotionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<ManagePromotionRequest> _validator;
    private readonly ILogger<PromotionService> _logger;

    public PromotionService(IUnitOfWork unitOfWork, IValidator<ManagePromotionRequest> validator, ILogger<PromotionService> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<PromotionResponse> AddAsync(ManagePromotionRequest request)
    {
        await ValidateAsync(request);
        var promotion = new Promotion
        {
            PromotionCode = CreatePromotionCode(),
            ProgramName = request.ProgramName.Trim(),
            PromotionType = request.PromotionType.Trim(),
            PromotionValue = request.PromotionValue,
            StartDateUtc = request.StartDateUtc,
            EndDateUtc = request.EndDateUtc,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        AddProducts(promotion, request.ProductIds);
        await _unitOfWork.Promotions.AddAsync(promotion);
        await AddHistoryAsync(promotion, request.ActorUserId, "Add", "Promotion added.");
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Manager {UserId} added promotion {PromotionCode}.", request.ActorUserId, promotion.PromotionCode);
        return Map(promotion, "Promotion added successfully.");
    }

    public async Task<PromotionResponse> UpdateAsync(ManagePromotionRequest request)
    {
        await ValidateAsync(request);
        if (!request.PromotionId.HasValue)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.PromotionId), "Promotion id is required.") });
        }

        var promotion = await _unitOfWork.Promotions.GetWithProductsAsync(request.PromotionId.Value);
        if (promotion is null) throw new NotFoundException("Promotion not found.");
        promotion.ProgramName = request.ProgramName.Trim();
        promotion.PromotionType = request.PromotionType.Trim();
        promotion.PromotionValue = request.PromotionValue;
        promotion.StartDateUtc = request.StartDateUtc;
        promotion.EndDateUtc = request.EndDateUtc;
        promotion.IsActive = request.IsActive;
        promotion.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.PromotionProducts.DeleteRange(promotion.PromotionProducts);
        promotion.PromotionProducts.Clear();
        AddProducts(promotion, request.ProductIds);
        _unitOfWork.Promotions.Update(promotion);
        await AddHistoryAsync(promotion, request.ActorUserId, "Update", "Promotion updated.");
        await _unitOfWork.SaveChangesAsync();
        return Map(promotion, "Promotion updated successfully.");
    }

    public async Task<PromotionResponse> DeleteAsync(int promotionId, int actorUserId, string actorRole)
    {
        EnsureManager(actorRole);
        if (await _unitOfWork.Promotions.HasActiveOrdersAsync(promotionId))
        {
            throw new InvalidOperationException("Promotion cannot be deleted because it is applied to active orders.");
        }

        var promotion = await _unitOfWork.Promotions.GetWithProductsAsync(promotionId);
        if (promotion is null) throw new NotFoundException("Promotion not found.");
        _unitOfWork.PromotionProducts.DeleteRange(promotion.PromotionProducts);
        _unitOfWork.Promotions.Delete(promotion);
        await AddHistoryAsync(promotion, actorUserId, "Delete", "Promotion deleted.");
        await _unitOfWork.SaveChangesAsync();
        return Map(promotion, "Promotion deleted successfully.");
    }

    private async Task ValidateAsync(ManagePromotionRequest request)
    {
        ThrowIfInvalid(await _validator.ValidateAsync(request));
        EnsureManager(request.ActorRole);
        foreach (var productId in request.ProductIds)
        {
            if (await _unitOfWork.Products.GetByIdAsync(productId) is null)
            {
                throw new NotFoundException("Product not found.");
            }
        }
    }

    private static void AddProducts(Promotion promotion, IEnumerable<int> productIds)
    {
        foreach (var productId in productIds.Distinct())
        {
            promotion.PromotionProducts.Add(new PromotionProduct
            {
                Promotion = promotion,
                ProductId = productId
            });
        }
    }

    private async Task AddHistoryAsync(Promotion promotion, int actorUserId, string operation, string content)
    {
        await _unitOfWork.PromotionManagementHistories.AddAsync(new PromotionManagementHistory
        {
            Promotion = promotion,
            PromotionId = promotion.PromotionId,
            ActorUserId = actorUserId,
            Operation = operation,
            ChangedContent = content,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    private static PromotionResponse Map(Promotion promotion, string message) => new()
    {
        PromotionId = promotion.PromotionId,
        PromotionCode = promotion.PromotionCode,
        ProgramName = promotion.ProgramName,
        PromotionType = promotion.PromotionType,
        PromotionValue = promotion.PromotionValue,
        StartDateUtc = promotion.StartDateUtc,
        EndDateUtc = promotion.EndDateUtc,
        IsActive = promotion.IsActive,
        ProductIds = promotion.PromotionProducts.Select(item => item.ProductId).ToArray(),
        Message = message
    };

    private static string CreatePromotionCode() => $"PROMO-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    private static void EnsureManager(string role)
    {
        if (!role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedException("Only Managers can manage promotions.");
    }
    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid) throw new ValidationException(result.Errors);
    }
}
