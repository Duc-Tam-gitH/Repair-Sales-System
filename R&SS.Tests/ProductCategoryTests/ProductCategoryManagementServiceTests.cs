using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.ProductCategory;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.ProductCategory;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.ProductCategoryTests;

public class ProductCategoryManagementServiceTests
{
    [Fact]
    public async Task AddAsync_ShouldCreateCategoryWithGeneratedCode_WhenRequestIsValid()
    {
        var mocks = CreateMocks();
        mocks.Categories.Setup(repo => repo.ExistsNameAsync("Laptop", null)).ReturnsAsync(false);
        var service = CreateService(mocks);

        var response = await service.AddAsync(BuildRequest());

        response.CategoryCode.Should().StartWith("CAT-");
        response.Message.Should().Be("Category added successfully.");
    }

    [Fact]
    public async Task AddAsync_ShouldThrowInvalidOperationException_WhenCategoryNameExists()
    {
        var mocks = CreateMocks();
        mocks.Categories.Setup(repo => repo.ExistsNameAsync("Laptop", null)).ReturnsAsync(true);
        var service = CreateService(mocks);

        var act = async () => await service.AddAsync(BuildRequest());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Category name already exists.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenCategoryHasProducts()
    {
        var mocks = CreateMocks();
        mocks.Categories.Setup(repo => repo.HasProductsAsync(1)).ReturnsAsync(true);
        var service = CreateService(mocks);

        var act = async () => await service.DeleteAsync(1, 2, RoleConstants.Manager);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Category cannot be deleted because it is linked to products.");
    }

    private static ProductCategoryManagementService CreateService(TestMocks mocks)
    {
        return new ProductCategoryManagementService(mocks.UnitOfWork.Object, new ManageProductCategoryRequestValidator(), Mock.Of<ILogger<ProductCategoryManagementService>>());
    }

    private static TestMocks CreateMocks()
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.ProductCategories).Returns(mocks.Categories.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.ProductCategoryManagementHistories).Returns(mocks.Histories.Object);
        mocks.UnitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mocks;
    }

    private static ManageProductCategoryRequest BuildRequest() => new()
    {
        ActorUserId = 2,
        ActorRole = RoleConstants.Manager,
        CategoryName = "Laptop",
        IsActive = true
    };

    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IProductCategoryRp> Categories { get; } = new();
        public Mock<IProductCategoryManagementHistoryRp> Histories { get; } = new();
    }
}
