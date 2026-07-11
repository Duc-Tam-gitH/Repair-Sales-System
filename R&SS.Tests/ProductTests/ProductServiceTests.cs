using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using R_SS.BLL.DTOs.Product;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Product;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.ProductTests;

public class ProductServiceTests
{
    [Fact]
    public async Task GetProductsAsync_ShouldReturnProducts_WhenProductsAreAvailable()
    {
        var product = BuildProduct();
        var productRepoMock = new Mock<IProductRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(productRepoMock);
        var mapperMock = CreateMapperMock(new[] { product });

        productRepoMock.Setup(repo => repo.GetActiveProductsAsync()).ReturnsAsync(new[] { product });
        var service = CreateService(unitOfWorkMock.Object, mapperMock.Object);

        var response = await service.GetProductsAsync();

        response.Products.Should().ContainSingle();
        response.Products.Single().ProductName.Should().Be("Laptop Battery");
        response.Message.Should().Be("Products retrieved successfully.");
    }

    [Fact]
    public async Task GetProductsAsync_ShouldReturnNoDataMessage_WhenNoProductsAreAvailable()
    {
        var productRepoMock = new Mock<IProductRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(productRepoMock);
        var mapperMock = CreateMapperMock(Array.Empty<Product>());

        productRepoMock.Setup(repo => repo.GetActiveProductsAsync()).ReturnsAsync(Array.Empty<Product>());
        var service = CreateService(unitOfWorkMock.Object, mapperMock.Object);

        var response = await service.GetProductsAsync();

        response.Products.Should().BeEmpty();
        response.Message.Should().Be("No products available.");
    }

    [Fact]
    public async Task GetProductsAsync_ShouldExposePriceAndStockQuantity()
    {
        var product = BuildProduct();
        var productRepoMock = new Mock<IProductRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(productRepoMock);
        var mapperMock = CreateMapperMock(new[] { product });

        productRepoMock.Setup(repo => repo.GetActiveProductsAsync()).ReturnsAsync(new[] { product });
        var service = CreateService(unitOfWorkMock.Object, mapperMock.Object);

        var result = await service.GetProductsAsync();

        result.Products.Single().SalePrice.Should().Be(45.50m);
        result.Products.Single().QuantityInStock.Should().Be(7);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingProducts_WhenCriteriaIsName()
    {
        var product = BuildProduct();
        var productRepoMock = new Mock<IProductRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(productRepoMock);
        var mapperMock = CreateMapperMock(new[] { product });

        productRepoMock.Setup(repo => repo.SearchActiveProductsAsync("battery", "name")).ReturnsAsync(new[] { product });
        var service = CreateService(unitOfWorkMock.Object, mapperMock.Object);

        var response = await service.SearchAsync(new SearchProductsRequest
        {
            Keyword = "battery",
            Criteria = "name"
        });

        response.Products.Should().ContainSingle();
        response.Message.Should().Be("Products retrieved successfully.");
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnNoMatchMessage_WhenNoProductsFound()
    {
        var productRepoMock = new Mock<IProductRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(productRepoMock);
        var mapperMock = CreateMapperMock(Array.Empty<Product>());

        productRepoMock.Setup(repo => repo.SearchActiveProductsAsync("missing", "all")).ReturnsAsync(Array.Empty<Product>());
        var service = CreateService(unitOfWorkMock.Object, mapperMock.Object);

        var response = await service.SearchAsync(new SearchProductsRequest
        {
            Keyword = "missing",
            Criteria = "all"
        });

        response.Products.Should().BeEmpty();
        response.Message.Should().Be("No matching products found.");
    }

    [Fact]
    public async Task SearchAsync_ShouldThrowValidationException_WhenCriteriaIsInvalid()
    {
        var service = CreateService(new Mock<IUnitOfWork>().Object, new Mock<IMapper>().Object);

        var act = async () => await service.SearchAsync(new SearchProductsRequest
        {
            Keyword = "battery",
            Criteria = "serial"
        });

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(error => error.PropertyName == nameof(SearchProductsRequest.Criteria));
    }

    [Fact]
    public async Task GetProductDetailsAsync_ShouldReturnDetails_WhenProductExists()
    {
        var product = BuildProduct();
        product.Description = "High quality replacement battery";
        var productRepoMock = new Mock<IProductRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(productRepoMock);
        var mapperMock = CreateMapperMock(new[] { product });

        productRepoMock.Setup(repo => repo.GetActiveProductByIdAsync(product.ProductId)).ReturnsAsync(product);
        var service = CreateService(unitOfWorkMock.Object, mapperMock.Object);

        var response = await service.GetProductDetailsAsync(product.ProductId);

        response.ProductName.Should().Be("Laptop Battery");
        response.BrandName.Should().Be("Acme");
        response.Description.Should().Be("High quality replacement battery");
        response.Message.Should().Be("Product details retrieved successfully.");
    }

    [Fact]
    public async Task GetProductDetailsAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        var productRepoMock = new Mock<IProductRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(productRepoMock);
        var service = CreateService(unitOfWorkMock.Object, new Mock<IMapper>().Object);

        productRepoMock.Setup(repo => repo.GetActiveProductByIdAsync(999)).ReturnsAsync((Product?)null);

        var act = async () => await service.GetProductDetailsAsync(999);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product not found.");
    }

    [Fact]
    public async Task GetProductDetailsAsync_ShouldThrowValidationException_WhenProductIdIsInvalid()
    {
        var service = CreateService(new Mock<IUnitOfWork>().Object, new Mock<IMapper>().Object);

        var act = async () => await service.GetProductDetailsAsync(0);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(error => error.PropertyName == "productId");
    }

    private static ProductService CreateService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        return new ProductService(
            unitOfWork,
            mapper,
            new SearchProductsRequestValidator(),
            Mock.Of<ILogger<ProductService>>());
    }

    private static Mock<IUnitOfWork> CreateUnitOfWorkMock(Mock<IProductRp> productRepoMock)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(unit => unit.Products).Returns(productRepoMock.Object);
        return unitOfWorkMock;
    }

    private static Mock<IMapper> CreateMapperMock(IReadOnlyCollection<Product> products)
    {
        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(mapper => mapper.Map<IReadOnlyCollection<ProductResponse>>(It.IsAny<object>()))
            .Returns(products.Select(MapProduct).ToArray());
        mapperMock
            .Setup(mapper => mapper.Map<ProductDetailResponse>(It.IsAny<Product>()))
            .Returns((Product product) => new ProductDetailResponse
            {
                ProductId = product.ProductId,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                SalePrice = product.SalePrice,
                QuantityInStock = product.QuantityInStock,
                CategoryName = product.ProductCategory?.CategoryName,
                BrandName = product.Supplier?.SupplierName,
                Description = product.Description
            });
        return mapperMock;
    }

    private static ProductResponse MapProduct(Product product)
    {
        return new ProductResponse
        {
            ProductId = product.ProductId,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            SalePrice = product.SalePrice,
            QuantityInStock = product.QuantityInStock,
            CategoryName = product.ProductCategory?.CategoryName,
            BrandName = product.Supplier?.SupplierName
        };
    }

    private static Product BuildProduct()
    {
        return new Product
        {
            ProductId = 1,
            ProductCode = "P001",
            ProductName = "Laptop Battery",
            SalePrice = 45.50m,
            QuantityInStock = 7,
            IsActive = true,
            ProductCategory = new ProductCategory { ProductCategoryId = 1, CategoryName = "Parts" },
            Supplier = new Supplier { SupplierId = 1, SupplierCode = "S001", SupplierName = "Acme" }
        };
    }
}
