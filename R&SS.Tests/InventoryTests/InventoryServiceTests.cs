using FluentAssertions;
using FluentValidation;
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

public class InventoryServiceTests
{
    [Fact]
    public async Task ApplyTransactionAsync_ShouldIncreaseStockAndLogTransaction_WhenReceiptIsValid()
    {
        var mocks = CreateMocks();
        var product = BuildProduct(stock: 5);
        mocks.Products.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);
        var service = CreateService(mocks);

        var response = await service.ApplyTransactionAsync(new InventoryTransactionRequest { ProductId = 1, ActorUserId = 2, ActorRole = RoleConstants.Manager, TransactionType = "Receipt", Quantity = 3 });

        response.StockAfter.Should().Be(8);
        product.QuantityInStock.Should().Be(8);
        mocks.Transactions.Verify(repo => repo.AddAsync(It.Is<InventoryTransaction>(transaction => transaction.QuantityChange == 3)), Times.Once);
    }

    [Fact]
    public async Task ApplyTransactionAsync_ShouldThrowInvalidOperationException_WhenIssueExceedsStock()
    {
        var mocks = CreateMocks();
        mocks.Products.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(BuildProduct(stock: 2));
        var service = CreateService(mocks);

        var act = async () => await service.ApplyTransactionAsync(new InventoryTransactionRequest { ProductId = 1, ActorUserId = 2, ActorRole = RoleConstants.Manager, TransactionType = "Issue", Quantity = 5 });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Stock is insufficient.");
    }

    [Fact]
    public async Task ApplyTransactionAsync_ShouldThrowUnauthorizedException_WhenActorIsNotManager()
    {
        var service = CreateService(CreateMocks());

        var act = async () => await service.ApplyTransactionAsync(new InventoryTransactionRequest { ProductId = 1, ActorUserId = 2, ActorRole = RoleConstants.Receptionist, TransactionType = "Receipt", Quantity = 1 });

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Only Managers can manage inventory.");
    }

    private static InventoryService CreateService(TestMocks mocks) => new(mocks.UnitOfWork.Object, new InventoryTransactionRequestValidator(), new InventoryHistoryRequestValidator());

    private static TestMocks CreateMocks()
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.Products).Returns(mocks.Products.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.InventoryTransactions).Returns(mocks.Transactions.Object);
        mocks.UnitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mocks;
    }

    private static Product BuildProduct(int stock) => new() { ProductId = 1, ProductCode = "P001", ProductName = "Battery", QuantityInStock = stock };

    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IProductRp> Products { get; } = new();
        public Mock<IInventoryTransactionRp> Transactions { get; } = new();
    }
}
