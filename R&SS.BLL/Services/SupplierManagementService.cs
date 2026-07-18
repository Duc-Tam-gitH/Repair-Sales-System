using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Supplier;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class SupplierManagementService : ISupplierManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<ManageSupplierRequest> _validator;
    private readonly ILogger<SupplierManagementService> _logger;

    public SupplierManagementService(IUnitOfWork unitOfWork, IValidator<ManageSupplierRequest> validator, ILogger<SupplierManagementService> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<SupplierListResponse> GetSuppliersAsync(string? keyword = null)
    {
        var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            suppliers = suppliers.Where(supplier =>
                supplier.SupplierCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                supplier.SupplierName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (supplier.ContactName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var mapped = suppliers
            .OrderBy(supplier => supplier.SupplierName)
            .Select(supplier => Map(supplier, string.Empty))
            .ToArray();

        return new SupplierListResponse
        {
            Suppliers = mapped,
            Message = mapped.Length == 0 ? "No suppliers found." : "Suppliers retrieved successfully."
        };
    }

    public async Task<SupplierResponse> AddAsync(ManageSupplierRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        await ValidateRequestAsync(request);
        await EnsureUniqueCodeAsync(request.SupplierCode);

        var supplier = new Supplier();
        Apply(supplier, request);
        supplier.CreatedAt = DateTime.UtcNow;
        supplier.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Suppliers.AddAsync(supplier);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Manager {UserId} added supplier {SupplierCode}.", request.ActorUserId, supplier.SupplierCode);

        return Map(supplier, "Supplier added successfully.");
    }

    public async Task<SupplierResponse> UpdateAsync(ManageSupplierRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        await ValidateRequestAsync(request);
        if (!request.SupplierId.HasValue)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.SupplierId), "Supplier id is required.") });
        }

        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(request.SupplierId.Value);
        if (supplier is null)
        {
            throw new NotFoundException("Supplier not found.");
        }

        await EnsureUniqueCodeAsync(request.SupplierCode, supplier.SupplierId);
        Apply(supplier, request);
        supplier.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Manager {UserId} updated supplier {SupplierCode}.", request.ActorUserId, supplier.SupplierCode);

        return Map(supplier, "Supplier updated successfully.");
    }

    public async Task<SupplierResponse> DisableAsync(int supplierId, int actorUserId, string actorRole)
    {
        EnsureManager(actorRole);
        if (actorUserId <= 0)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(actorUserId), "Actor user id is required.") });
        }

        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(supplierId);
        if (supplier is null)
        {
            throw new NotFoundException("Supplier not found.");
        }

        supplier.IsActive = false;
        supplier.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Manager {UserId} disabled supplier {SupplierCode}.", actorUserId, supplier.SupplierCode);

        return Map(supplier, "Supplier disabled successfully.");
    }

    private async Task ValidateRequestAsync(ManageSupplierRequest request)
    {
        ThrowIfInvalid(await _validator.ValidateAsync(request));
        EnsureManager(request.ActorRole);
    }

    private async Task EnsureUniqueCodeAsync(string supplierCode, int? excludedSupplierId = null)
    {
        var normalized = supplierCode.Trim();
        var exists = await _unitOfWork.Suppliers.AnyAsync(supplier =>
            supplier.SupplierCode == normalized && (!excludedSupplierId.HasValue || supplier.SupplierId != excludedSupplierId.Value));
        if (exists)
        {
            throw new InvalidOperationException("Supplier code already exists.");
        }
    }

    private static void Apply(Supplier supplier, ManageSupplierRequest request)
    {
        supplier.SupplierCode = request.SupplierCode.Trim();
        supplier.SupplierName = request.SupplierName.Trim();
        supplier.ContactName = Normalize(request.ContactName);
        supplier.Phone = Normalize(request.Phone);
        supplier.Email = Normalize(request.Email);
        supplier.Address = Normalize(request.Address);
        supplier.TaxCode = Normalize(request.TaxCode);
        supplier.Notes = Normalize(request.Notes);
        supplier.IsActive = request.IsActive;
    }

    private static SupplierResponse Map(Supplier supplier, string message) => new()
    {
        SupplierId = supplier.SupplierId,
        SupplierCode = supplier.SupplierCode,
        SupplierName = supplier.SupplierName,
        ContactName = supplier.ContactName,
        Phone = supplier.Phone,
        Email = supplier.Email,
        Address = supplier.Address,
        TaxCode = supplier.TaxCode,
        Notes = supplier.Notes,
        IsActive = supplier.IsActive,
        Message = message
    };

    private static void EnsureManager(string role)
    {
        if (!role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Managers can manage suppliers.");
        }
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid) throw new ValidationException(result.Errors);
    }
}
