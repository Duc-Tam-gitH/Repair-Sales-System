using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Customer;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Customer;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.CustomerTests;

public class CustomerServiceTests
{
    [Fact]
    public async Task SearchAsync_ShouldReturnCustomers_WhenActorIsReceptionist()
    {
        var customer = BuildCustomer();
        var customerRepoMock = new Mock<ICustomerRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(customerRepoMock);
        var mapperMock = CreateMapperMock(new[] { customer });

        customerRepoMock.Setup(repo => repo.SearchAsync("john")).ReturnsAsync(new[] { customer });
        var service = CreateService(unitOfWorkMock.Object, mapperMock.Object);

        var response = await service.SearchAsync(new CustomerSearchRequest
        {
            Keyword = "john",
            ActorRole = RoleConstants.Receptionist
        });

        response.Customers.Should().ContainSingle();
        response.Message.Should().Be("Customers retrieved successfully.");
    }

    [Fact]
    public async Task SearchAsync_ShouldThrowUnauthorizedException_WhenActorRoleIsNotAllowed()
    {
        var service = CreateService(new Mock<IUnitOfWork>().Object, new Mock<IMapper>().Object);

        var act = async () => await service.SearchAsync(new CustomerSearchRequest
        {
            ActorRole = RoleConstants.Client
        });

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only Receptionist or Manager roles can access customer management.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenPhoneAlreadyExists()
    {
        var customerRepoMock = new Mock<ICustomerRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(customerRepoMock);
        var service = CreateService(unitOfWorkMock.Object, new Mock<IMapper>().Object);

        customerRepoMock.Setup(repo => repo.ExistsCodeAsync("C001", null)).ReturnsAsync(false);
        customerRepoMock.Setup(repo => repo.ExistsPhoneAsync("0123456789", null)).ReturnsAsync(true);

        var act = async () => await service.CreateAsync(new CreateCustomerRequest
        {
            CustomerCode = "C001",
            FullName = "John Doe",
            Phone = "0123456789",
            Email = "john@example.com",
            ActorRole = RoleConstants.Manager,
            ActorUserId = 10
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Phone number already exists.");
        customerRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCustomerAndRecordHistory_WhenRequestIsValid()
    {
        var customer = BuildCustomer();
        var customerRepoMock = new Mock<ICustomerRp>();
        var historyRepoMock = new Mock<ICustomerUpdateHistoryRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(customerRepoMock, historyRepoMock);
        var mapperMock = CreateMapperMock(new[] { customer });

        customerRepoMock.Setup(repo => repo.GetByIdAsync(customer.CustomerId)).ReturnsAsync(customer);
        customerRepoMock.Setup(repo => repo.ExistsCodeAsync(customer.CustomerCode, customer.CustomerId)).ReturnsAsync(false);
        customerRepoMock.Setup(repo => repo.ExistsPhoneAsync("0987654321", customer.CustomerId)).ReturnsAsync(false);
        customerRepoMock.Setup(repo => repo.ExistsEmailAsync("new@example.com", customer.CustomerId)).ReturnsAsync(false);

        var service = CreateService(unitOfWorkMock.Object, mapperMock.Object);

        var response = await service.UpdateAsync(new UpdateCustomerRequest
        {
            CustomerId = customer.CustomerId,
            FullName = "John Updated",
            Phone = "0987654321",
            Email = "new@example.com",
            Address = "New Address",
            Notes = "Updated note",
            ActorRole = RoleConstants.Manager,
            ActorUserId = 10
        });

        response.Message.Should().Be("Customer updated successfully.");
        customer.FullName.Should().Be("John Updated");
        historyRepoMock.Verify(repo => repo.AddAsync(It.Is<CustomerUpdateHistory>(history =>
            history.UpdatedByUserId == 10 &&
            history.ChangedContent.Contains("FullName"))), Times.Once);
        unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CustomerService CreateService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        return new CustomerService(
            unitOfWork,
            mapper,
            new CustomerSearchRequestValidator(),
            new CreateCustomerRequestValidator(),
            new UpdateCustomerRequestValidator(),
            Mock.Of<ILogger<CustomerService>>());
    }

    private static Mock<IUnitOfWork> CreateUnitOfWorkMock(
        Mock<ICustomerRp> customerRepoMock,
        Mock<ICustomerUpdateHistoryRp>? historyRepoMock = null)
    {
        historyRepoMock ??= new Mock<ICustomerUpdateHistoryRp>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(unit => unit.Customers).Returns(customerRepoMock.Object);
        unitOfWorkMock.SetupGet(unit => unit.CustomerUpdateHistories).Returns(historyRepoMock.Object);
        unitOfWorkMock.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return unitOfWorkMock;
    }

    private static Mock<IMapper> CreateMapperMock(IReadOnlyCollection<Customer> customers)
    {
        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(mapper => mapper.Map<IReadOnlyCollection<CustomerResponse>>(It.IsAny<object>()))
            .Returns(customers.Select(MapCustomer).ToArray());
        mapperMock
            .Setup(mapper => mapper.Map<CustomerResponse>(It.IsAny<Customer>()))
            .Returns((Customer customer) => MapCustomer(customer));
        return mapperMock;
    }

    private static CustomerResponse MapCustomer(Customer customer)
    {
        return new CustomerResponse
        {
            CustomerId = customer.CustomerId,
            CustomerCode = customer.CustomerCode,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            Notes = customer.Notes,
            IsActive = customer.IsActive
        };
    }

    private static Customer BuildCustomer()
    {
        return new Customer
        {
            CustomerId = 1,
            CustomerCode = "C001",
            FullName = "John Doe",
            Phone = "0123456789",
            Email = "john@example.com",
            Address = "Old Address",
            Notes = "Old note",
            IsActive = true
        };
    }
}
