using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Product;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Product;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.ProductTests;

public class ProductManagementServiceTests
{
    [Fact]
    public async Task AddAsync_ShouldCreateProductAndHistory_WhenRequestIsValid()
    {
        var mocks = CreateMocks();
        mocks.Products.Setup(repo => repo.ExistsCodeAsync("P001", null)).ReturnsAsync(false);
        var service = CreateService(mocks);

        var response = await service.AddAsync(BuildRequest());

        response.Message.Should().Be("Product added successfully.");
        mocks.Products.Verify(repo => repo.AddAsync(It.Is<Product>(product => product.ProductCode == "P001")), Times.Once);
        mocks.Histories.Verify(repo => repo.AddAsync(It.Is<ProductManagementHistory>(history => history.Operation == "Add")), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowInvalidOperationException_WhenProductCodeExists()
    {
        var mocks = CreateMocks();
        mocks.Products.Setup(repo => repo.ExistsCodeAsync("P001", null)).ReturnsAsync(true);
        var service = CreateService(mocks);

        var act = async () => await service.AddAsync(BuildRequest());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Product code already exists.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenProductHasReferences()
    {
        var mocks = CreateMocks();
        mocks.Products.Setup(repo => repo.HasReferencesAsync(1)).ReturnsAsync(true);
        var service = CreateService(mocks);

        var act = async () => await service.DeleteAsync(1, 2, RoleConstants.Manager);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Product cannot be deleted because it is referenced by orders or repair tickets.");
    }

    private static ProductManagementService CreateService(TestMocks mocks)
    {
        return new ProductManagementService(mocks.UnitOfWork.Object, new ManageProductRequestValidator(), Mock.Of<ILogger<ProductManagementService>>());
    }

    private static TestMocks CreateMocks()
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.Products).Returns(mocks.Products.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.ProductManagementHistories).Returns(mocks.Histories.Object);
        mocks.UnitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mocks;
    }

    private static ManageProductRequest BuildRequest() => new()
    {
        ActorUserId = 2,
        ActorRole = RoleConstants.Manager,
        ProductCode = "P001",
        ProductName = "Keyboard",
        ProductCategoryId = 1,
        SalePrice = 30,
        QuantityInStock = 10
    };

    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IProductRp> Products { get; } = new();
        public Mock<IProductManagementHistoryRp> Histories { get; } = new();
    }
}
