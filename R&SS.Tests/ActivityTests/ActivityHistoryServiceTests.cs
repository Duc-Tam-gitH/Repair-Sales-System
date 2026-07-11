using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using R_SS.BLL.DTOs.Activity;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Activity;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.ActivityTests;

public class ActivityHistoryServiceTests
{
    [Fact]
    public async Task GetHistoryAsync_ShouldReturnOrdersAndTechnicalTickets_WhenActivitiesExist()
    {
        var mocks = CreateMocks();
        mocks.Customers.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(BuildCustomer());
        mocks.SalesOrders.Setup(repo => repo.GetByCustomerIdAsync(1)).ReturnsAsync(new[] { BuildSalesOrder() });
        mocks.RepairOrders.Setup(repo => repo.GetSubmittedByCustomerIdAsync(1, false)).ReturnsAsync(new[] { BuildRepairOrder() });
        var service = CreateService(mocks.UnitOfWork.Object);

        var response = await service.GetHistoryAsync(new ActivityHistoryRequest { CustomerId = 1, Type = "all" });

        response.Activities.Should().HaveCount(2);
        response.Activities.Should().Contain(activity => activity.Type == "order");
        response.Activities.Should().Contain(activity => activity.Type == "technical");
        response.Message.Should().Be("Activity history retrieved successfully.");
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldReturnNoActivitiesMessage_WhenNoneExist()
    {
        var mocks = CreateMocks();
        mocks.Customers.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(BuildCustomer());
        mocks.SalesOrders.Setup(repo => repo.GetByCustomerIdAsync(1)).ReturnsAsync(Array.Empty<SalesOrder>());
        mocks.RepairOrders.Setup(repo => repo.GetSubmittedByCustomerIdAsync(1, false)).ReturnsAsync(Array.Empty<RepairOrder>());
        var service = CreateService(mocks.UnitOfWork.Object);

        var response = await service.GetHistoryAsync(new ActivityHistoryRequest { CustomerId = 1, Type = "all" });

        response.Activities.Should().BeEmpty();
        response.Message.Should().Be("There are no activities.");
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldThrowValidationException_WhenTypeIsInvalid()
    {
        var service = CreateService(new Mock<IUnitOfWork>().Object);

        var act = async () => await service.GetHistoryAsync(new ActivityHistoryRequest { CustomerId = 1, Type = "invoice" });

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(error => error.PropertyName == nameof(ActivityHistoryRequest.Type));
    }

    [Fact]
    public async Task GetSalesOrderDetailsAsync_ShouldReturnDetails_WhenOrderBelongsToCustomer()
    {
        var order = BuildSalesOrder();
        var mocks = CreateMocks();
        mocks.SalesOrders.Setup(repo => repo.GetWithDetailsAsync(order.SalesOrderId)).ReturnsAsync(order);
        var service = CreateService(mocks.UnitOfWork.Object);

        var response = await service.GetSalesOrderDetailsAsync(1, order.SalesOrderId);

        response.SalesOrderCode.Should().Be("SO001");
        response.Message.Should().Be("Sales order details retrieved successfully.");
    }

    [Fact]
    public async Task GetRepairOrderDetailsAsync_ShouldThrowNotFoundException_WhenTicketBelongsToAnotherCustomer()
    {
        var repairOrder = BuildRepairOrder();
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(repairOrder.RepairOrderId)).ReturnsAsync(repairOrder);
        var service = CreateService(mocks.UnitOfWork.Object);

        var act = async () => await service.GetRepairOrderDetailsAsync(99, repairOrder.RepairOrderId);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Repair order not found.");
    }

    private static ActivityHistoryService CreateService(IUnitOfWork unitOfWork)
    {
        return new ActivityHistoryService(
            unitOfWork,
            new ActivityHistoryRequestValidator(),
            Mock.Of<ILogger<ActivityHistoryService>>());
    }

    private static TestMocks CreateMocks()
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.Customers).Returns(mocks.Customers.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.SalesOrders).Returns(mocks.SalesOrders.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.RepairOrders).Returns(mocks.RepairOrders.Object);
        return mocks;
    }

    private static Customer BuildCustomer()
    {
        return new Customer { CustomerId = 1, CustomerCode = "C001", FullName = "John Doe" };
    }

    private static SalesOrder BuildSalesOrder()
    {
        return new SalesOrder
        {
            SalesOrderId = 10,
            CustomerId = 1,
            SalesOrderCode = "SO001",
            Status = "Pending Confirmation",
            CreatedAt = DateTime.UtcNow,
            TotalAmount = 100
        };
    }

    private static RepairOrder BuildRepairOrder()
    {
        return new RepairOrder
        {
            RepairOrderId = 20,
            CustomerId = 1,
            RepairOrderCode = "RO001",
            IssueDescription = "Screen issue",
            Status = "Received",
            CreatedAt = DateTime.UtcNow
        };
    }

    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<ICustomerRp> Customers { get; } = new();
        public Mock<ISalesOrderRp> SalesOrders { get; } = new();
        public Mock<IRepairOrderRp> RepairOrders { get; } = new();
    }
}
