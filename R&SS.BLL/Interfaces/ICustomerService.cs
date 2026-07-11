using R_SS.BLL.DTOs.Customer;

namespace R_SS.BLL.Interfaces;

public interface ICustomerService
{
    /// <summary>
    /// Searches or lists customers for an authorized staff user.
    /// </summary>
    Task<CustomerListResponse> SearchAsync(CustomerSearchRequest request);

    /// <summary>
    /// Gets customer details for an authorized staff user.
    /// </summary>
    Task<CustomerResponse> GetByIdAsync(int customerId, string actorRole);

    /// <summary>
    /// Creates a customer profile.
    /// </summary>
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request);

    /// <summary>
    /// Updates customer information and records update history.
    /// </summary>
    Task<CustomerResponse> UpdateAsync(UpdateCustomerRequest request);
}
