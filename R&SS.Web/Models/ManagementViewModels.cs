using R_SS.BLL.DTOs.Account;
using R_SS.Models.Entities;

namespace R_SS.Web.Models;

public class ManagementViewModel
{
    public IReadOnlyCollection<User> Users { get; set; } = Array.Empty<User>();
    public IReadOnlyCollection<Role> Roles { get; set; } = Array.Empty<Role>();
    public IReadOnlyCollection<Employee> Employees { get; set; } = Array.Empty<Employee>();
    public IReadOnlyCollection<Customer> Customers { get; set; } = Array.Empty<Customer>();
    public IReadOnlyCollection<ServiceRequest> ServiceRequests { get; set; } = Array.Empty<ServiceRequest>();
    public IReadOnlyCollection<RepairOrder> RepairOrders { get; set; } = Array.Empty<RepairOrder>();
    public IReadOnlyCollection<SalesOrder> SalesOrders { get; set; } = Array.Empty<SalesOrder>();
    public ManageAccountRequest Account { get; set; } = new();
}
