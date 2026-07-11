using FluentValidation;
using FluentValidation.Results;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Invoice;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.BLL.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<PrintInvoiceRequest> _validator;

    public InvoiceService(IUnitOfWork unitOfWork, IEmailSender emailSender, IValidator<PrintInvoiceRequest> validator)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _validator = validator;
    }

    public async Task<InvoiceResponse> PrintAsync(PrintInvoiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(await _validator.ValidateAsync(request));
        EnsureStaff(request.ActorRole);

        return request.TransactionType.Equals("Sales", StringComparison.OrdinalIgnoreCase)
            ? await PrintSalesAsync(request)
            : await PrintRepairAsync(request);
    }

    private async Task<InvoiceResponse> PrintSalesAsync(PrintInvoiceRequest request)
    {
        var order = await _unitOfWork.SalesOrders.GetWithDetailsAsync(request.TransactionId);
        if (order is null)
        {
            throw new NotFoundException("Sales order not found.");
        }

        if (!order.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
            !order.Payments.Any(payment => payment.PaymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Only confirmed sales transactions can be invoiced.");
        }

        var invoice = await SaveInvoiceAsync(request, salesOrderId: order.SalesOrderId, repairOrderId: null);
        if (request.SendEmail && !string.IsNullOrWhiteSpace(order.Customer?.Email))
        {
            await _emailSender.SendInvoiceAsync(order.Customer.Email, order.Customer.FullName, invoice.InvoiceCode);
            invoice.SentToEmail = order.Customer.Email;
            _unitOfWork.InvoiceRecords.Update(invoice);
            await _unitOfWork.SaveChangesAsync();
        }

        return new InvoiceResponse
        {
            InvoiceRecordId = invoice.InvoiceRecordId,
            InvoiceCode = invoice.InvoiceCode,
            TransactionType = "Sales",
            TransactionId = order.SalesOrderId,
            CustomerName = order.Customer?.FullName ?? string.Empty,
            TotalAmount = order.TotalAmount,
            PdfPath = invoice.PdfPath,
            PrintedAtUtc = invoice.CreatedAtUtc,
            Message = "Invoice printed successfully."
        };
    }

    private async Task<InvoiceResponse> PrintRepairAsync(PrintInvoiceRequest request)
    {
        var order = await _unitOfWork.RepairOrders.GetWithDetailsAsync(request.TransactionId);
        if (order is null)
        {
            throw new NotFoundException("Technical ticket not found.");
        }

        if (!order.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
            !order.Payments.Any(payment => payment.PaymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Only confirmed repair transactions can be invoiced.");
        }

        var invoice = await SaveInvoiceAsync(request, salesOrderId: null, repairOrderId: order.RepairOrderId);
        if (request.SendEmail && !string.IsNullOrWhiteSpace(order.Customer?.Email))
        {
            await _emailSender.SendInvoiceAsync(order.Customer.Email, order.Customer.FullName, invoice.InvoiceCode);
            invoice.SentToEmail = order.Customer.Email;
            _unitOfWork.InvoiceRecords.Update(invoice);
            await _unitOfWork.SaveChangesAsync();
        }

        return new InvoiceResponse
        {
            InvoiceRecordId = invoice.InvoiceRecordId,
            InvoiceCode = invoice.InvoiceCode,
            TransactionType = "Repair",
            TransactionId = order.RepairOrderId,
            CustomerName = order.Customer?.FullName ?? string.Empty,
            TotalAmount = order.TotalAmount,
            PdfPath = invoice.PdfPath,
            PrintedAtUtc = invoice.CreatedAtUtc,
            Message = "Invoice printed successfully."
        };
    }

    private async Task<InvoiceRecord> SaveInvoiceAsync(PrintInvoiceRequest request, int? salesOrderId, int? repairOrderId)
    {
        var invoice = new InvoiceRecord
        {
            InvoiceCode = await CreateUniqueCodeAsync(),
            SalesOrderId = salesOrderId,
            RepairOrderId = repairOrderId,
            ActorUserId = request.ActorUserId,
            InvoiceType = request.TransactionType.Trim(),
            PdfPath = request.ExportPdf ? $"invoices/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}.pdf" : null,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.InvoiceRecords.AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();
        return invoice;
    }

    private async Task<string> CreateUniqueCodeAsync()
    {
        string code;
        do
        {
            code = $"INV-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }
        while (await _unitOfWork.InvoiceRecords.ExistsCodeAsync(code));

        return code;
    }

    private static void EnsureStaff(string role)
    {
        if (!role.Equals(RoleConstants.Receptionist, StringComparison.OrdinalIgnoreCase) &&
            !role.Equals(RoleConstants.Manager, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Only Receptionist or Manager roles can print invoices.");
        }
    }

    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
