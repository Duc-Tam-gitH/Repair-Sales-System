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

    ICustomerRp Customers { get; }

    ISupplierRp Suppliers { get; }

    IProductCategoryRp ProductCategories { get; }

    IProductRp Products { get; }

    IPurchaseOrderRp PurchaseOrders { get; }

    IPurchaseOrderDetailRp PurchaseOrderDetails { get; }

    ISalesOrderRp SalesOrders { get; }

    ISalesOrderDetailRp SalesOrderDetails { get; }

    IRepairOrderRp RepairOrders { get; }

    IRepairOrderDetailRp RepairOrderDetails { get; }

    IPaymentRp Payments { get; }

    IDbContextTransaction BeginTransaction();

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    void Commit();

    Task CommitAsync(CancellationToken cancellationToken = default);

    void Rollback();

    Task RollbackAsync(CancellationToken cancellationToken = default);

    int SaveChanges();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
