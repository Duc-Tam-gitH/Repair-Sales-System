using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;

namespace R_SS.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDalServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repository
        services.AddScoped<IUserRp, UserRp>();
        services.AddScoped<IRoleRp, RoleRp>();
        services.AddScoped<IUserRoleRp, UserRoleRp>();
        services.AddScoped<IPasswordResetRequestRp, PasswordResetRequestRp>();
        services.AddScoped<ICustomerRp, CustomerRp>();
        services.AddScoped<ICustomerUpdateHistoryRp, CustomerUpdateHistoryRp>();
        services.AddScoped<ISupplierRp, SupplierRp>();
        services.AddScoped<IProductCategoryRp, ProductCategoryRp>();
        services.AddScoped<IProductCategoryManagementHistoryRp, ProductCategoryManagementHistoryRp>();
        services.AddScoped<IProductRp, ProductRp>();
        services.AddScoped<IProductManagementHistoryRp, ProductManagementHistoryRp>();
        services.AddScoped<IPromotionRp, PromotionRp>();
        services.AddScoped<IPromotionProductRp, PromotionProductRp>();
        services.AddScoped<IPromotionManagementHistoryRp, PromotionManagementHistoryRp>();
        services.AddScoped<IInventoryTransactionRp, InventoryTransactionRp>();
        services.AddScoped<ISystemActivityLogRp, SystemActivityLogRp>();
        services.AddScoped<IServiceRequestRp, ServiceRequestRp>();
        services.AddScoped<IInvoiceRecordRp, InvoiceRecordRp>();
        services.AddScoped<IRolePermissionRp, RolePermissionRp>();
        services.AddScoped<ISystemConfigurationRp, SystemConfigurationRp>();
        services.AddScoped<INotificationTemplateRp, NotificationTemplateRp>();
        services.AddScoped<INotificationTemplateHistoryRp, NotificationTemplateHistoryRp>();
        services.AddScoped<IPurchaseOrderRp, PurchaseOrderRp>();
        services.AddScoped<IPurchaseOrderDetailRp, PurchaseOrderDetailRp>();
        services.AddScoped<ISalesOrderRp, SalesOrderRp>();
        services.AddScoped<ISalesOrderDetailRp, SalesOrderDetailRp>();
        services.AddScoped<IRepairOrderRp, RepairOrderRp>();
        services.AddScoped<IRepairOrderDetailRp, RepairOrderDetailRp>();
        services.AddScoped<IRepairOrderStatusHistoryRp, RepairOrderStatusHistoryRp>();
        services.AddScoped<ITechnicianAssignmentHistoryRp, TechnicianAssignmentHistoryRp>();
        services.AddScoped<IServiceFeedbackRp, ServiceFeedbackRp>();
        services.AddScoped<IPaymentRp, PaymentRp>();
        services.AddScoped<ICartRp, CartRp>();
        services.AddScoped<ICartItemRp, CartItemRp>();

        // Unit Of Work
        services.AddScoped<IUnitOfWork, R_SS.DAL.UnitOfWork.UnitOfWork>();

        return services;
    }
}
