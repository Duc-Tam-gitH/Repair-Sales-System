using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using R_SS.BLL.DTOs.Cart;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Cart;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.CartTests;

public class CartServiceTests
{
    [Fact]
    public async Task AddToCartAsync_ShouldCreateCartAndItem_WhenCartDoesNotExist()
    {
        var product = BuildProduct();
        var customerRepoMock = new Mock<ICustomerRp>();
        var productRepoMock = new Mock<IProductRp>();
        var cartRepoMock = new Mock<ICartRp>();
        var cartItemRepoMock = new Mock<ICartItemRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(customerRepoMock, productRepoMock, cartRepoMock, cartItemRepoMock);

        customerRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(new Customer { CustomerId = 1, CustomerCode = "C001", FullName = "John" });
        productRepoMock.Setup(repo => repo.GetActiveProductByIdAsync(2)).ReturnsAsync(product);
        cartRepoMock.Setup(repo => repo.GetByCustomerIdAsync(1)).ReturnsAsync((Cart?)null);

        var service = CreateService(unitOfWorkMock.Object);

        var response = await service.AddToCartAsync(new AddToCartRequest { CustomerId = 1, ProductId = 2, Quantity = 2 });

        response.TotalQuantity.Should().Be(2);
        response.TotalAmount.Should().Be(20);
        cartRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Cart>()), Times.Once);
        cartItemRepoMock.Verify(repo => repo.AddAsync(It.IsAny<CartItem>()), Times.Once);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldUpdateQuantity_WhenProductAlreadyExistsInCart()
    {
        var product = BuildProduct();
        var cart = new Cart { CartId = 5, CustomerId = 1 };
        var item = new CartItem { CartItemId = 7, CartId = 5, ProductId = 2, Product = product, Quantity = 1, UnitPrice = 10 };
        cart.CartItems.Add(item);
        var customerRepoMock = new Mock<ICustomerRp>();
        var productRepoMock = new Mock<IProductRp>();
        var cartRepoMock = new Mock<ICartRp>();
        var cartItemRepoMock = new Mock<ICartItemRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(customerRepoMock, productRepoMock, cartRepoMock, cartItemRepoMock);

        customerRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(new Customer { CustomerId = 1, CustomerCode = "C001", FullName = "John" });
        productRepoMock.Setup(repo => repo.GetActiveProductByIdAsync(2)).ReturnsAsync(product);
        cartRepoMock.Setup(repo => repo.GetByCustomerIdAsync(1)).ReturnsAsync(cart);
        cartItemRepoMock.Setup(repo => repo.GetByCartAndProductAsync(5, 2)).ReturnsAsync(item);

        var service = CreateService(unitOfWorkMock.Object);

        var response = await service.AddToCartAsync(new AddToCartRequest { CustomerId = 1, ProductId = 2, Quantity = 3 });

        item.Quantity.Should().Be(4);
        response.TotalQuantity.Should().Be(4);
        cartItemRepoMock.Verify(repo => repo.Update(item), Times.Once);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldThrowInvalidOperationException_WhenQuantityExceedsStock()
    {
        var product = BuildProduct();
        product.QuantityInStock = 1;
        var customerRepoMock = new Mock<ICustomerRp>();
        var productRepoMock = new Mock<IProductRp>();
        var cartRepoMock = new Mock<ICartRp>();
        var cartItemRepoMock = new Mock<ICartItemRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(customerRepoMock, productRepoMock, cartRepoMock, cartItemRepoMock);

        customerRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(new Customer { CustomerId = 1, CustomerCode = "C001", FullName = "John" });
        productRepoMock.Setup(repo => repo.GetActiveProductByIdAsync(2)).ReturnsAsync(product);
        cartRepoMock.Setup(repo => repo.GetByCustomerIdAsync(1)).ReturnsAsync((Cart?)null);

        var service = CreateService(unitOfWorkMock.Object);

        var act = async () => await service.AddToCartAsync(new AddToCartRequest { CustomerId = 1, ProductId = 2, Quantity = 2 });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Stock is insufficient.");
        cartItemRepoMock.Verify(repo => repo.AddAsync(It.IsAny<CartItem>()), Times.Never);
    }

    private static CartService CreateService(IUnitOfWork unitOfWork)
    {
        return new CartService(unitOfWork, new AddToCartRequestValidator(), Mock.Of<ILogger<CartService>>());
    }

    private static Mock<IUnitOfWork> CreateUnitOfWorkMock(
        Mock<ICustomerRp> customerRepoMock,
        Mock<IProductRp> productRepoMock,
        Mock<ICartRp> cartRepoMock,
        Mock<ICartItemRp> cartItemRepoMock)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(unit => unit.Customers).Returns(customerRepoMock.Object);
        unitOfWorkMock.SetupGet(unit => unit.Products).Returns(productRepoMock.Object);
        unitOfWorkMock.SetupGet(unit => unit.Carts).Returns(cartRepoMock.Object);
        unitOfWorkMock.SetupGet(unit => unit.CartItems).Returns(cartItemRepoMock.Object);
        unitOfWorkMock.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return unitOfWorkMock;
    }

    private static Product BuildProduct()
    {
        return new Product
        {
            ProductId = 2,
            ProductCode = "P002",
            ProductName = "USB Cable",
            SalePrice = 10,
            QuantityInStock = 5,
            IsActive = true
        };
    }
}
