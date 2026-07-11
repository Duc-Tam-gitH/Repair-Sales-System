using FluentAssertions;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Inventory;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Inventory;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.InventoryTests;

public class InventoryStatisticServiceTests
{
    [Fact]
    public async Task GenerateAsync_ShouldReturnReceiptIssueAndStockQuantities()
    {
        var mocks = CreateMocks();
        mocks.Products.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new[] { BuildProduct(stock: 4, reorder: 5) });
        mocks.Transactions.Setup(repo => repo.SearchAsync(null, null, null, null)).ReturnsAsync(new[]
        {
            new InventoryTransaction { ProductId = 1, QuantityChange = 7, TransactionType = "Receipt" },
            new InventoryTransaction { ProductId = 1, QuantityChange = -3, TransactionType = "Issue" }
        });
        var service = CreateService(mocks);

        var response = await service.GenerateAsync(new InventoryStatisticRequest { ActorRole = RoleConstants.Manager });

        response.Items.Single().ReceiptQuantity.Should().Be(7);
        response.Items.Single().IssueQuantity.Should().Be(3);
        response.Items.Single().InventoryStatus.Should().Be("Low Stock");
    }

    [Fact]
    public async Task GenerateAsync_ShouldFilterLowStock_WhenRequested()
    {
        var mocks = CreateMocks();
        mocks.Products.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new[] { BuildProduct(stock: 10, reorder: 2) });
        mocks.Transactions.Setup(repo => repo.SearchAsync(null, null, null, null)).ReturnsAsync(Array.Empty<InventoryTransaction>());
        var service = CreateService(mocks);

        var response = await service.GenerateAsync(new InventoryStatisticRequest { ActorRole = RoleConstants.Manager, StockStatus = "low" });

        response.Items.Should().BeEmpty();
        response.Message.Should().Be("No statistical data.");
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowUnauthorizedException_WhenActorIsNotManager()
    {
        var service = CreateService(CreateMocks());

        var act = async () => await service.GenerateAsync(new InventoryStatisticRequest { ActorRole = RoleConstants.Customer });

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Only Managers can view inventory statistics.");
    }

    private static InventoryStatisticService CreateService(TestMocks mocks) => new(mocks.UnitOfWork.Object, new InventoryStatisticRequestValidator());
    private static TestMocks CreateMocks()
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.Products).Returns(mocks.Products.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.InventoryTransactions).Returns(mocks.Transactions.Object);
        return mocks;
    }
    private static Product BuildProduct(int stock, int reorder) => new() { ProductId = 1, ProductCode = "P001", ProductName = "Battery", QuantityInStock = stock, ReorderLevel = reorder };
    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IProductRp> Products { get; } = new();
        public Mock<IInventoryTransactionRp> Transactions { get; } = new();
    }
}
