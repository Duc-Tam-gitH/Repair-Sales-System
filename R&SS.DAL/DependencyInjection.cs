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
        services.AddScoped<ICustomerRp, CustomerRp>();
        services.AddScoped<ISupplierRp, SupplierRp>();
        services.AddScoped<IProductCategoryRp, ProductCategoryRp>();
        services.AddScoped<IProductRp, ProductRp>();
        services.AddScoped<IPurchaseOrderRp, PurchaseOrderRp>();
        services.AddScoped<IPurchaseOrderDetailRp, PurchaseOrderDetailRp>();
        services.AddScoped<ISalesOrderRp, SalesOrderRp>();
        services.AddScoped<ISalesOrderDetailRp, SalesOrderDetailRp>();
        services.AddScoped<IRepairOrderRp, RepairOrderRp>();
        services.AddScoped<IRepairOrderDetailRp, RepairOrderDetailRp>();
        services.AddScoped<IPaymentRp, PaymentRp>();

        // Unit Of Work
        services.AddScoped<IUnitOfWork, R_SS.DAL.UnitOfWork.UnitOfWork>();

        return services;
    }
}
