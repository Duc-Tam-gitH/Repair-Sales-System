using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using R_SS.BLL.Interfaces;
using R_SS.BLL.Helpers;
using R_SS.BLL.Services;

namespace R_SS.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Register Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IActivityHistoryService, ActivityHistoryService>();
        services.AddScoped<ITechnicalTicketService, TechnicalTicketService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IProductManagementService, ProductManagementService>();
        services.AddScoped<IProductCategoryManagementService, ProductCategoryManagementService>();
        services.AddScoped<ISupplierManagementService, SupplierManagementService>();
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IAccountManagementService, AccountManagementService>();
        services.AddScoped<ISystemActivityLogService, SystemActivityLogService>();
        services.AddScoped<IRevenueReportService, RevenueReportService>();
        services.AddScoped<IInventoryStatisticService, InventoryStatisticService>();
        services.AddScoped<IServiceRequestService, ServiceRequestService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IDeliveryConfirmationService, DeliveryConfirmationService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
        services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IOtpGenerator, OtpGenerator>();
        services.AddAutoMapper(_ => { }, typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }

}
