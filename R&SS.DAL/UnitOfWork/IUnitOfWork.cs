using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using R_SS.DAL.Repositories.Interfaces;

namespace R_SS.DAL.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IUserRp Users { get; }

    IRoleRp Roles { get; }

    IUserRoleRp UserRoles { get; }

    IPasswordResetRequestRp PasswordResetRequests { get; }

    ICustomerRp Customers { get; }

    IEmployeeRp Employees { get; }

    ICustomerUpdateHistoryRp CustomerUpdateHistories { get; }

    ISupplierRp Suppliers { get; }

    IProductCategoryRp ProductCategories { get; }

    IProductCategoryManagementHistoryRp ProductCategoryManagementHistories { get; }

    IProductRp Products { get; }

    IProductManagementHistoryRp ProductManagementHistories { get; }

    IPromotionRp Promotions { get; }

    IPromotionProductRp PromotionProducts { get; }

    IPromotionManagementHistoryRp PromotionManagementHistories { get; }

    IInventoryTransactionRp InventoryTransactions { get; }

    ISystemActivityLogRp SystemActivityLogs { get; }

    IServiceRequestRp ServiceRequests { get; }

    IInvoiceRecordRp InvoiceRecords { get; }

    IRolePermissionRp RolePermissions { get; }

    ISystemConfigurationRp SystemConfigurations { get; }

    INotificationTemplateRp NotificationTemplates { get; }

    INotificationTemplateHistoryRp NotificationTemplateHistories { get; }

    IPurchaseOrderRp PurchaseOrders { get; }

    IPurchaseOrderDetailRp PurchaseOrderDetails { get; }

    ISalesOrderRp SalesOrders { get; }

    ISalesOrderDetailRp SalesOrderDetails { get; }

    IRepairOrderRp RepairOrders { get; }

    IRepairOrderDetailRp RepairOrderDetails { get; }

    IRepairOrderStatusHistoryRp RepairOrderStatusHistories { get; }

    ITechnicianAssignmentHistoryRp TechnicianAssignmentHistories { get; }

    IServiceFeedbackRp ServiceFeedbacks { get; }

    IPaymentRp Payments { get; }

    ICartRp Carts { get; }

    ICartItemRp CartItems { get; }

    IDbContextTransaction BeginTransaction();

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    void Commit();

    Task CommitAsync(CancellationToken cancellationToken = default);

    void Rollback();

    Task RollbackAsync(CancellationToken cancellationToken = default);

    int SaveChanges();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
