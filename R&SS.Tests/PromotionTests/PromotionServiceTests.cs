using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Promotion;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Promotion;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.PromotionTests;

public class PromotionServiceTests
{
    [Fact]
    public async Task AddAsync_ShouldCreatePromotionAndHistory_WhenRequestIsValid()
    {
        var mocks = CreateMocks();
        mocks.Products.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(new Product { ProductId = 1, ProductCode = "P001", ProductName = "Keyboard" });
        var service = CreateService(mocks);

        var response = await service.AddAsync(BuildRequest());

        response.PromotionCode.Should().StartWith("PROMO-");
        response.ProductIds.Should().Contain(1);
        mocks.Promotions.Verify(repo => repo.AddAsync(It.IsAny<Promotion>()), Times.Once);
        mocks.Histories.Verify(repo => repo.AddAsync(It.Is<PromotionManagementHistory>(history => history.Operation == "Add")), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowValidationException_WhenEndDateIsBeforeStartDate()
    {
        var service = CreateService(CreateMocks());
        var request = BuildRequest();
        request.EndDateUtc = request.StartDateUtc.AddDays(-1);

        var act = async () => await service.AddAsync(request);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(error => error.PropertyName == nameof(ManagePromotionRequest.EndDateUtc));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenPromotionHasActiveOrders()
    {
        var mocks = CreateMocks();
        mocks.Promotions.Setup(repo => repo.HasActiveOrdersAsync(1)).ReturnsAsync(true);
        var service = CreateService(mocks);

        var act = async () => await service.DeleteAsync(1, 2, RoleConstants.Manager);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Promotion cannot be deleted because it is applied to active orders.");
    }

    private static PromotionService CreateService(TestMocks mocks)
    {
        return new PromotionService(mocks.UnitOfWork.Object, new ManagePromotionRequestValidator(), Mock.Of<ILogger<PromotionService>>());
    }

    private static TestMocks CreateMocks()
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.Promotions).Returns(mocks.Promotions.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.PromotionProducts).Returns(mocks.PromotionProducts.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.PromotionManagementHistories).Returns(mocks.Histories.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.Products).Returns(mocks.Products.Object);
        mocks.UnitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mocks;
    }

    private static ManagePromotionRequest BuildRequest() => new()
    {
        ActorUserId = 2,
        ActorRole = RoleConstants.Manager,
        ProgramName = "Summer Sale",
        PromotionType = "Percentage",
        PromotionValue = 10,
        StartDateUtc = DateTime.UtcNow,
        EndDateUtc = DateTime.UtcNow.AddDays(7),
        ProductIds = new[] { 1 }
    };

    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IPromotionRp> Promotions { get; } = new();
        public Mock<IPromotionProductRp> PromotionProducts { get; } = new();
        public Mock<IPromotionManagementHistoryRp> Histories { get; } = new();
        public Mock<IProductRp> Products { get; } = new();
    }
}
