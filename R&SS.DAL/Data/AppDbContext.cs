using Microsoft.EntityFrameworkCore;
using R_SS.Models.Entities;
using System.Reflection;

namespace R_SS.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerUpdateHistory> CustomerUpdateHistories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductManagementHistory> ProductManagementHistories { get; set; }
        public DbSet<ProductCategoryManagementHistory> ProductCategoryManagementHistories { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionProduct> PromotionProducts { get; set; }
        public DbSet<PromotionManagementHistory> PromotionManagementHistories { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<SystemActivityLog> SystemActivityLogs { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<InvoiceRecord> InvoiceRecords { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<NotificationTemplateHistory> NotificationTemplateHistories { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
        public DbSet<RepairOrder> RepairOrders { get; set; }
        public DbSet<RepairOrderStatusHistory> RepairOrderStatusHistories { get; set; }
        public DbSet<TechnicianAssignmentHistory> TechnicianAssignmentHistories { get; set; }
        public DbSet<ServiceFeedback> ServiceFeedbacks { get; set; }
        public DbSet<RepairOrderDetail> RepairOrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
        Assembly.GetExecutingAssembly());
        }
    }
}
