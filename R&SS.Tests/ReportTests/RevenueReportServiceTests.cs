using FluentAssertions;
using FluentValidation;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Report;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Report;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.ReportTests;

public class RevenueReportServiceTests
{
    [Fact]
    public async Task GenerateAsync_ShouldAggregateSalesAndRepairRevenue()
    {
        var mocks = CreateMocks();
        mocks.Payments.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new[]
        {
            new Payment { Amount = 100, PaymentStatus = "Completed", PaymentDate = DateTime.UtcNow, SalesOrderId = 1, PaymentCode = "S1" },
            new Payment { Amount = 50, PaymentStatus = "Completed", PaymentDate = DateTime.UtcNow, RepairOrderId = 1, PaymentCode = "R1" }
        });
        var service = CreateService(mocks);

        var response = await service.GenerateAsync(BuildRequest());

        response.TotalRevenue.Should().Be(150);
        response.ProductSalesRevenue.Should().Be(100);
        response.RepairServiceRevenue.Should().Be(50);
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowValidationException_WhenDateRangeInvalid()
    {
        var service = CreateService(CreateMocks());
        var request = BuildRequest();
        request.FromUtc = DateTime.UtcNow;
        request.ToUtc = request.FromUtc.AddDays(-1);

        var act = async () => await service.GenerateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowUnauthorizedException_WhenActorIsNotManager()
    {
        var service = CreateService(CreateMocks());
        var request = BuildRequest();
        request.ActorRole = RoleConstants.Client;

        var act = async () => await service.GenerateAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Only Managers can generate revenue reports.");
    }

    private static RevenueReportService CreateService(TestMocks mocks) => new(mocks.UnitOfWork.Object, new RevenueReportRequestValidator());
    private static TestMocks CreateMocks()
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.Payments).Returns(mocks.Payments.Object);
        return mocks;
    }
    private static RevenueReportRequest BuildRequest() => new() { ActorRole = RoleConstants.Manager, FromUtc = DateTime.UtcNow.AddDays(-1), ToUtc = DateTime.UtcNow.AddDays(1) };
    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IPaymentRp> Payments { get; } = new();
    }
}
