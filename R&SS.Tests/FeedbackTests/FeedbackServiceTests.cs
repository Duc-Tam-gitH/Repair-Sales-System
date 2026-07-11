using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Feedback;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Feedback;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.FeedbackTests;

public class FeedbackServiceTests
{
    [Fact]
    public async Task GetEligibleItemsAsync_ShouldReturnCompletedOrdersAndDeliveredTickets()
    {
        var mocks = CreateMocks();
        mocks.Customers.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(BuildCustomer());
        mocks.SalesOrders.Setup(repo => repo.GetByCustomerIdAsync(1)).ReturnsAsync(new[] { BuildOrder("Completed") });
        mocks.RepairOrders.Setup(repo => repo.GetSubmittedByCustomerIdAsync(1, false)).ReturnsAsync(new[] { BuildTicket("Delivered") });
        mocks.Feedbacks.Setup(repo => repo.ExistsForSalesOrderAsync(10)).ReturnsAsync(false);
        mocks.Feedbacks.Setup(repo => repo.ExistsForRepairOrderAsync(20)).ReturnsAsync(false);
        var service = CreateService(mocks);

        var response = await service.GetEligibleItemsAsync(1);

        response.Items.Should().HaveCount(2);
        response.Message.Should().Be("Eligible feedback items retrieved successfully.");
    }

    [Fact]
    public async Task GetEligibleItemsAsync_ShouldReturnNoDataMessage_WhenNothingEligible()
    {
        var mocks = CreateMocks();
        mocks.Customers.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(BuildCustomer());
        mocks.SalesOrders.Setup(repo => repo.GetByCustomerIdAsync(1)).ReturnsAsync(Array.Empty<SalesOrder>());
        mocks.RepairOrders.Setup(repo => repo.GetSubmittedByCustomerIdAsync(1, false)).ReturnsAsync(Array.Empty<RepairOrder>());
        var service = CreateService(mocks);

        var response = await service.GetEligibleItemsAsync(1);

        response.Items.Should().BeEmpty();
        response.Message.Should().Be("No eligible items for rating.");
    }

    [Fact]
    public async Task SubmitAsync_ShouldSaveOrderFeedback_WhenOrderIsCompleted()
    {
        var mocks = CreateMocks();
        mocks.SalesOrders.Setup(repo => repo.GetWithDetailsAsync(10)).ReturnsAsync(BuildOrder("Completed"));
        mocks.Feedbacks.Setup(repo => repo.ExistsForSalesOrderAsync(10)).ReturnsAsync(false);
        var service = CreateService(mocks);

        var response = await service.SubmitAsync(new SubmitFeedbackRequest
        {
            CustomerId = 1,
            Type = "order",
            ItemId = 10,
            Rating = 5,
            Comment = "Great service"
        });

        response.Message.Should().Be("Feedback sent successfully.");
        mocks.Feedbacks.Verify(repo => repo.AddAsync(It.Is<ServiceFeedback>(feedback => feedback.Rating == 5)), Times.Once);
    }

    [Fact]
    public async Task SubmitAsync_ShouldThrowInvalidOperationException_WhenItemAlreadyRated()
    {
        var mocks = CreateMocks();
        mocks.SalesOrders.Setup(repo => repo.GetWithDetailsAsync(10)).ReturnsAsync(BuildOrder("Completed"));
        mocks.Feedbacks.Setup(repo => repo.ExistsForSalesOrderAsync(10)).ReturnsAsync(true);
        var service = CreateService(mocks);

        var act = async () => await service.SubmitAsync(new SubmitFeedbackRequest
        {
            CustomerId = 1,
            Type = "order",
            ItemId = 10,
            Rating = 4
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("This item has already been rated.");
    }

    [Fact]
    public async Task SubmitAsync_ShouldThrowValidationException_WhenRatingIsOutOfRange()
    {
        var service = CreateService(CreateMocks());

        var act = async () => await service.SubmitAsync(new SubmitFeedbackRequest
        {
            CustomerId = 1,
            Type = "technical",
            ItemId = 20,
            Rating = 6
        });

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(error => error.PropertyName == nameof(SubmitFeedbackRequest.Rating));
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnAverageRating_WhenActorIsManager()
    {
        var mocks = CreateMocks();
        mocks.Feedbacks.Setup(repo => repo.GetAllWithReferencesAsync()).ReturnsAsync(new[]
        {
            new ServiceFeedback { Rating = 4 },
            new ServiceFeedback { Rating = 5 }
        });
        var service = CreateService(mocks);

        var response = await service.GetStatisticsAsync(RoleConstants.Manager);

        response.TotalFeedbackCount.Should().Be(2);
        response.AverageRating.Should().Be(4.5);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldThrowUnauthorizedException_WhenActorIsNotManager()
    {
        var service = CreateService(CreateMocks());

        var act = async () => await service.GetStatisticsAsync(RoleConstants.Customer);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only Managers can view feedback statistics.");
    }

    private static FeedbackService CreateService(TestMocks mocks)
    {
        return new FeedbackService(
            mocks.UnitOfWork.Object,
            new SubmitFeedbackRequestValidator(),
            Mock.Of<ILogger<FeedbackService>>());
    }

    private static TestMocks CreateMocks()
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.Customers).Returns(mocks.Customers.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.SalesOrders).Returns(mocks.SalesOrders.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.RepairOrders).Returns(mocks.RepairOrders.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.ServiceFeedbacks).Returns(mocks.Feedbacks.Object);
        mocks.UnitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mocks;
    }

    private static Customer BuildCustomer() => new() { CustomerId = 1, CustomerCode = "C001", FullName = "John Doe" };

    private static SalesOrder BuildOrder(string status) => new()
    {
        SalesOrderId = 10,
        CustomerId = 1,
        SalesOrderCode = "SO001",
        Status = status,
        UpdatedAt = DateTime.UtcNow
    };

    private static RepairOrder BuildTicket(string status) => new()
    {
        RepairOrderId = 20,
        CustomerId = 1,
        RepairOrderCode = "TT001",
        Status = status,
        IssueDescription = "Screen issue",
        DeliveryConfirmedAtUtc = DateTime.UtcNow
    };

    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<ICustomerRp> Customers { get; } = new();
        public Mock<ISalesOrderRp> SalesOrders { get; } = new();
        public Mock<IRepairOrderRp> RepairOrders { get; } = new();
        public Mock<IServiceFeedbackRp> Feedbacks { get; } = new();
    }
}
