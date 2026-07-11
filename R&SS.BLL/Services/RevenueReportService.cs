using FluentValidation;
using FluentValidation.Results;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Report;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class RevenueReportService : IRevenueReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RevenueReportRequest> _validator;

    public RevenueReportService(IUnitOfWork unitOfWork, IValidator<RevenueReportRequest> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<RevenueReportResponse> GenerateAsync(RevenueReportRequest request)
    {
        ThrowIfInvalid(await _validator.ValidateAsync(request));
        EnsureManager(request.ActorRole);
        var payments = (await _unitOfWork.Payments.GetAllAsync())
            .Where(payment => payment.PaymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) &&
                payment.PaymentDate >= request.FromUtc &&
                payment.PaymentDate <= request.ToUtc)
            .ToArray();
        var productRevenue = payments.Where(payment => payment.SalesOrderId.HasValue).Sum(payment => payment.Amount);
        var repairRevenue = payments.Where(payment => payment.RepairOrderId.HasValue).Sum(payment => payment.Amount);
        if (request.RevenueType.Equals("sales", StringComparison.OrdinalIgnoreCase)) repairRevenue = 0;
        if (request.RevenueType.Equals("repair", StringComparison.OrdinalIgnoreCase)) productRevenue = 0;
        return new RevenueReportResponse
        {
            ProductSalesRevenue = productRevenue,
            RepairServiceRevenue = repairRevenue,
            TotalRevenue = productRevenue + repairRevenue,
            TransactionCount = payments.Length,
            ExportFileName = CreateExportName(request),
            Message = payments.Length == 0 ? "No revenue data exists in the selected range." : "Revenue report generated successfully."
        };
    }

    private static string? CreateExportName(RevenueReportRequest request)
    {
        if (request.ExportFormat.Equals("pdf", StringComparison.OrdinalIgnoreCase) || request.ExportFormat.Equals("excel", StringComparison.OrdinalIgnoreCase))
        {
            return $"RevenueReport-{DateTime.UtcNow:yyyyMMddHHmmss}.{request.ExportFormat.ToLower()}";
        }

        return null;
    }

    private static void EnsureManager(string role)
    {
        if (!role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedException("Only Managers can generate revenue reports.");
    }
    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid) throw new ValidationException(result.Errors);
    }
}
