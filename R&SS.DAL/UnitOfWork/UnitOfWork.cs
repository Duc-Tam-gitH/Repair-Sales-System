using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories;
using R_SS.DAL.Repositories.Interfaces;

namespace R_SS.DAL.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;

        Users = new UserRp(_context);
        Roles = new RoleRp(_context);
        UserRoles = new UserRoleRp(_context);
        PasswordResetRequests = new PasswordResetRequestRp(_context);
        Customers = new CustomerRp(_context);
        CustomerUpdateHistories = new CustomerUpdateHistoryRp(_context);
        Suppliers = new SupplierRp(_context);
        ProductCategories = new ProductCategoryRp(_context);
        ProductCategoryManagementHistories = new ProductCategoryManagementHistoryRp(_context);
        Products = new ProductRp(_context);
        ProductManagementHistories = new ProductManagementHistoryRp(_context);
        Promotions = new PromotionRp(_context);
        PromotionProducts = new PromotionProductRp(_context);
        PromotionManagementHistories = new PromotionManagementHistoryRp(_context);
        InventoryTransactions = new InventoryTransactionRp(_context);
        SystemActivityLogs = new SystemActivityLogRp(_context);
        ServiceRequests = new ServiceRequestRp(_context);
        InvoiceRecords = new InvoiceRecordRp(_context);
        RolePermissions = new RolePermissionRp(_context);
        SystemConfigurations = new SystemConfigurationRp(_context);
        NotificationTemplates = new NotificationTemplateRp(_context);
        NotificationTemplateHistories = new NotificationTemplateHistoryRp(_context);
        PurchaseOrders = new PurchaseOrderRp(_context);
        PurchaseOrderDetails = new PurchaseOrderDetailRp(_context);
        SalesOrders = new SalesOrderRp(_context);
        SalesOrderDetails = new SalesOrderDetailRp(_context);
        RepairOrders = new RepairOrderRp(_context);
        RepairOrderDetails = new RepairOrderDetailRp(_context);
        RepairOrderStatusHistories = new RepairOrderStatusHistoryRp(_context);
        TechnicianAssignmentHistories = new TechnicianAssignmentHistoryRp(_context);
        ServiceFeedbacks = new ServiceFeedbackRp(_context);
        Payments = new PaymentRp(_context);
        Carts = new CartRp(_context);
        CartItems = new CartItemRp(_context);
    }

    public IUserRp Users { get; }

    public IRoleRp Roles { get; }

    public IUserRoleRp UserRoles { get; }

    public IPasswordResetRequestRp PasswordResetRequests { get; }

    public ICustomerRp Customers { get; }

    public ICustomerUpdateHistoryRp CustomerUpdateHistories { get; }

    public ISupplierRp Suppliers { get; }

    public IProductCategoryRp ProductCategories { get; }

    public IProductCategoryManagementHistoryRp ProductCategoryManagementHistories { get; }

    public IProductRp Products { get; }

    public IProductManagementHistoryRp ProductManagementHistories { get; }

    public IPromotionRp Promotions { get; }

    public IPromotionProductRp PromotionProducts { get; }

    public IPromotionManagementHistoryRp PromotionManagementHistories { get; }

    public IInventoryTransactionRp InventoryTransactions { get; }

    public ISystemActivityLogRp SystemActivityLogs { get; }

    public IServiceRequestRp ServiceRequests { get; }

    public IInvoiceRecordRp InvoiceRecords { get; }

    public IRolePermissionRp RolePermissions { get; }

    public ISystemConfigurationRp SystemConfigurations { get; }

    public INotificationTemplateRp NotificationTemplates { get; }

    public INotificationTemplateHistoryRp NotificationTemplateHistories { get; }

    public IPurchaseOrderRp PurchaseOrders { get; }

    public IPurchaseOrderDetailRp PurchaseOrderDetails { get; }

    public ISalesOrderRp SalesOrders { get; }

    public ISalesOrderDetailRp SalesOrderDetails { get; }

    public IRepairOrderRp RepairOrders { get; }

    public IRepairOrderDetailRp RepairOrderDetails { get; }

    public IRepairOrderStatusHistoryRp RepairOrderStatusHistories { get; }

    public ITechnicianAssignmentHistoryRp TechnicianAssignmentHistories { get; }

    public IServiceFeedbackRp ServiceFeedbacks { get; }

    public IPaymentRp Payments { get; }

    public ICartRp Carts { get; }

    public ICartItemRp CartItems { get; }

    public IDbContextTransaction BeginTransaction()
    {
        _transaction ??= _context.Database.BeginTransaction();
        return _transaction;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return BeginTransactionAsyncInternal(cancellationToken);
    }

    public void Commit()
    {
        CommitTransaction();
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    public void Rollback()
    {
        RollbackTransaction();
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return RollbackTransactionAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
        }

        _disposed = true;
    }

    private async Task<IDbContextTransaction> BeginTransactionAsyncInternal(CancellationToken cancellationToken)
    {
        _transaction ??= await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        return _transaction;
    }

    private void CommitTransaction()
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("A transaction must be started before calling Commit().");
        }

        _transaction.Commit();
        _transaction.Dispose();
        _transaction = null;
    }

    private async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("A transaction must be started before calling CommitAsync().");
        }

        await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        await _transaction.DisposeAsync().ConfigureAwait(false);
        _transaction = null;
    }

    private void RollbackTransaction()
    {
        if (_transaction is null)
        {
            return;
        }

        _transaction.Rollback();
        _transaction.Dispose();
        _transaction = null;
    }

    private async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        await _transaction.DisposeAsync().ConfigureAwait(false);
        _transaction = null;
    }
}
