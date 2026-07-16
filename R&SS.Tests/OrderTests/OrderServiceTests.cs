using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Order;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Order;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.OrderTests;

public class OrderServiceTests
{
    [Fact]
    public async Task PlaceOrderAsync_ShouldCreateOrderAndClearCart_WhenCartIsValid()
    {
        var product = BuildProduct(stock: 5);
        var cart = BuildCart(product, quantity: 2);
        var mocks = CreateMocks();
        mocks.Customers.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(BuildCustomer());
        mocks.Carts.Setup(repo => repo.GetByCustomerIdAsync(1)).ReturnsAsync(cart);
        var service = CreateService(mocks);

        var response = await service.PlaceOrderAsync(new PlaceOrderRequest
        {
            CustomerId = 1,
            RecipientName = "John Doe",
            DeliveryAddress = "123 Main Street",
            PaymentMethod = "COD"
        });

        response.Status.Should().Be("Pending Confirmation");
        response.TotalAmount.Should().Be(50);
        product.QuantityInStock.Should().Be(3);
        mocks.SalesOrders.Verify(repo => repo.AddAsync(It.IsAny<SalesOrder>()), Times.Once);
        mocks.Payments.Verify(repo => repo.AddAsync(It.IsAny<Payment>()), Times.Once);
        mocks.CartItems.Verify(repo => repo.DeleteRange(cart.CartItems), Times.Once);
        mocks.UnitOfWork.Verify(unit => unit.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlaceOrderAsync_ShouldThrowInvalidOperationException_WhenCartIsEmpty()
    {
        var mocks = CreateMocks();
        mocks.Customers.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(BuildCustomer());
        mocks.Carts.Setup(repo => repo.GetByCustomerIdAsync(1)).ReturnsAsync(new Cart { CartId = 1, CustomerId = 1 });
        var service = CreateService(mocks);

        var act = async () => await service.PlaceOrderAsync(new PlaceOrderRequest
        {
            CustomerId = 1,
            RecipientName = "John Doe",
            DeliveryAddress = "123 Main Street",
            PaymentMethod = "COD"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cart is empty.");
        mocks.UnitOfWork.Verify(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PlaceOrderAsync_ShouldThrowInvalidOperationException_WhenCartQuantityExceedsStock()
    {
        var product = BuildProduct(stock: 1);
        var cart = BuildCart(product, quantity: 2);
        var mocks = CreateMocks();
        mocks.Customers.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(BuildCustomer());
        mocks.Carts.Setup(repo => repo.GetByCustomerIdAsync(1)).ReturnsAsync(cart);
        var service = CreateService(mocks);

        var act = async () => await service.PlaceOrderAsync(new PlaceOrderRequest
        {
            CustomerId = 1,
            RecipientName = "John Doe",
            DeliveryAddress = "123 Main Street",
            PaymentMethod = "COD"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Stock is insufficient.");
        mocks.SalesOrders.Verify(repo => repo.AddAsync(It.IsAny<SalesOrder>()), Times.Never);
    }

    [Fact]
    public async Task ProcessSalesOrderAsync_ShouldCreateCompletedOrderAndUpdateStock_WhenRequestIsValid()
    {
        var product = BuildProduct(stock: 6);
        var mocks = CreateMocks();
        mocks.Customers.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(BuildCustomer());
        mocks.Products.Setup(repo => repo.GetActiveProductByIdAsync(product.ProductId)).ReturnsAsync(product);
        var service = CreateService(mocks);

        var response = await service.ProcessSalesOrderAsync(new ProcessSalesOrderRequest
        {
            CustomerId = 1,
            ActorUserId = 9,
            ActorRole = RoleConstants.Receptionist,
            PaymentMethod = "Cash",
            Items = new[] { new OrderItemRequest { ProductId = product.ProductId, Quantity = 3 } }
        });

        response.Status.Should().Be("Completed");
        response.TotalAmount.Should().Be(75);
        product.QuantityInStock.Should().Be(3);
        mocks.Payments.Verify(repo => repo.AddAsync(It.Is<Payment>(payment => payment.PaymentStatus == "Completed")), Times.Once);
    }

    [Fact]
    public async Task ProcessSalesOrderAsync_ShouldThrowUnauthorizedException_WhenActorRoleIsNotAllowed()
    {
        var service = CreateService(new TestMocks());

        var act = async () => await service.ProcessSalesOrderAsync(new ProcessSalesOrderRequest
        {
            CustomerId = 1,
            ActorUserId = 9,
            ActorRole = RoleConstants.Client,
            PaymentMethod = "Cash",
            Items = new[] { new OrderItemRequest { ProductId = 1, Quantity = 1 } }
        });

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only Receptionist, Manager, or Administrator roles can process sales orders.");
    }

    [Fact]
    public async Task ProcessSalesOrderAsync_ShouldThrowInvalidOperationException_WhenStockIsInsufficient()
    {
        var product = BuildProduct(stock: 1);
        var mocks = CreateMocks();
        mocks.Customers.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(BuildCustomer());
        mocks.Products.Setup(repo => repo.GetActiveProductByIdAsync(product.ProductId)).ReturnsAsync(product);
        var service = CreateService(mocks);

        var act = async () => await service.ProcessSalesOrderAsync(new ProcessSalesOrderRequest
        {
            CustomerId = 1,
            ActorUserId = 9,
            ActorRole = RoleConstants.Manager,
            PaymentMethod = "Cash",
            Items = new[] { new OrderItemRequest { ProductId = product.ProductId, Quantity = 2 } }
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Stock is insufficient.");
        mocks.SalesOrders.Verify(repo => repo.AddAsync(It.IsAny<SalesOrder>()), Times.Never);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldCancelOrderAndRestoreInventory_WhenRequestIsValid()
    {
        var product = BuildProduct(stock: 2);
        var order = BuildSalesOrder(product, "Pending Confirmation");
        var mocks = CreateMocks();
        mocks.SalesOrders.Setup(repo => repo.GetWithDetailsAsync(order.SalesOrderId)).ReturnsAsync(order);
        var service = CreateService(mocks);

        var response = await service.CancelOrderAsync(new CancelOrderRequest
        {
            SalesOrderId = order.SalesOrderId,
            ActorUserId = 3,
            ActorRole = RoleConstants.Manager,
            Reason = "Customer requested cancellation"
        });

        response.Status.Should().Be("Cancelled");
        product.QuantityInStock.Should().Be(4);
        mocks.SystemActivityLogs.Verify(repo => repo.AddAsync(It.Is<SystemActivityLog>(log => log.FunctionName == "Cancel Order")), Times.Once);
        mocks.UnitOfWork.Verify(unit => unit.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldThrowUnauthorizedException_WhenActorIsReceptionist()
    {
        var service = CreateService(CreateMocks());

        var act = async () => await service.CancelOrderAsync(new CancelOrderRequest
        {
            SalesOrderId = 1,
            ActorUserId = 3,
            ActorRole = RoleConstants.Receptionist,
            Reason = "Invalid"
        });

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only Manager or Administrator roles can cancel orders.");
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldThrowInvalidOperationException_WhenOrderIsPaid()
    {
        var product = BuildProduct(stock: 2);
        var order = BuildSalesOrder(product, "Pending Confirmation");
        order.Payments.Add(new Payment { PaymentStatus = "Completed", PaymentCode = "P001", PaymentMethod = "Cash" });
        var mocks = CreateMocks();
        mocks.SalesOrders.Setup(repo => repo.GetWithDetailsAsync(order.SalesOrderId)).ReturnsAsync(order);
        var service = CreateService(mocks);

        var act = async () => await service.CancelOrderAsync(new CancelOrderRequest
        {
            SalesOrderId = order.SalesOrderId,
            ActorUserId = 3,
            ActorRole = RoleConstants.Admin,
            Reason = "Paid order"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Order cannot be cancelled.");
    }

    private static OrderService CreateService(TestMocks mocks)
    {
        return new OrderService(
            mocks.UnitOfWork.Object,
            mocks.EmailSender.Object,
            new PlaceOrderRequestValidator(),
            new ProcessSalesOrderRequestValidator(),
            new CancelOrderRequestValidator(),
            Mock.Of<ILogger<OrderService>>());
    }

    private static TestMocks CreateMocks()
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.Customers).Returns(mocks.Customers.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.Products).Returns(mocks.Products.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.Carts).Returns(mocks.Carts.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.CartItems).Returns(mocks.CartItems.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.SalesOrders).Returns(mocks.SalesOrders.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.Payments).Returns(mocks.Payments.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.SystemActivityLogs).Returns(mocks.SystemActivityLogs.Object);
        mocks.UnitOfWork.Setup(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Mock.Of<IDbContextTransaction>());
        mocks.UnitOfWork.Setup(unit => unit.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mocks.UnitOfWork.Setup(unit => unit.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mocks.UnitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mocks;
    }

    private static Customer BuildCustomer()
    {
        return new Customer { CustomerId = 1, CustomerCode = "C001", FullName = "John Doe" };
    }

    private static Product BuildProduct(int stock)
    {
        return new Product
        {
            ProductId = 11,
            ProductCode = "P011",
            ProductName = "SSD",
            SalePrice = 25,
            QuantityInStock = stock,
            IsActive = true
        };
    }

    private static Cart BuildCart(Product product, int quantity)
    {
        var cart = new Cart { CartId = 3, CustomerId = 1 };
        cart.CartItems.Add(new CartItem
        {
            CartItemId = 4,
            CartId = 3,
            ProductId = product.ProductId,
            Product = product,
            Quantity = quantity,
            UnitPrice = product.SalePrice
        });
        return cart;
    }

    private static SalesOrder BuildSalesOrder(Product product, string status)
    {
        var customer = BuildCustomer();
        customer.Email = "john@example.com";
        var order = new SalesOrder
        {
            SalesOrderId = 8,
            SalesOrderCode = "SO-008",
            CustomerId = 1,
            Customer = customer,
            Status = status,
            TotalAmount = product.SalePrice * 2
        };
        order.SalesOrderDetails.Add(new SalesOrderDetail
        {
            SalesOrderId = order.SalesOrderId,
            SalesOrder = order,
            ProductId = product.ProductId,
            Product = product,
            Quantity = 2,
            UnitPrice = product.SalePrice,
            LineTotal = product.SalePrice * 2
        });
        return order;
    }

    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<ICustomerRp> Customers { get; } = new();
        public Mock<IProductRp> Products { get; } = new();
        public Mock<ICartRp> Carts { get; } = new();
        public Mock<ICartItemRp> CartItems { get; } = new();
        public Mock<ISalesOrderRp> SalesOrders { get; } = new();
        public Mock<IPaymentRp> Payments { get; } = new();
        public Mock<ISystemActivityLogRp> SystemActivityLogs { get; } = new();
        public Mock<IEmailSender> EmailSender { get; } = new();
    }
}
