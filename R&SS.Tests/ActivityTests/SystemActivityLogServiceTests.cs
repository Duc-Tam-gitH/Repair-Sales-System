using FluentAssertions;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Activity;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Activity;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.ActivityTests;

public class SystemActivityLogServiceTests
{
    [Fact]
    public async Task SearchAsync_ShouldReturnLogs_WhenActorIsManager()
    {
        var mocks = CreateMocks();
        mocks.Logs.Setup(repo => repo.SearchAsync(null, null, null, null, null)).ReturnsAsync(new[] { BuildLog() });
        var service = CreateService(mocks);

        var response = await service.SearchAsync(new SystemActivityLogSearchRequest { ActorRole = RoleConstants.Manager });

        response.Logs.Should().ContainSingle();
        response.Message.Should().Be("Activity history retrieved successfully.");
    }

    [Fact]
    public async Task SearchAsync_ShouldThrowUnauthorizedException_WhenActorIsNotManager()
    {
        var service = CreateService(CreateMocks());

        var act = async () => await service.SearchAsync(new SystemActivityLogSearchRequest { ActorRole = RoleConstants.Customer });

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Only Managers can access activity history.");
    }

    [Fact]
    public async Task GetDetailsAsync_ShouldThrowNotFoundException_WhenLogMissing()
    {
        var mocks = CreateMocks();
        mocks.Logs.Setup(repo => repo.GetByIdAsync(99)).ReturnsAsync((SystemActivityLog?)null);
        var service = CreateService(mocks);

        var act = async () => await service.GetDetailsAsync(99, RoleConstants.Manager);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Activity history record not found.");
    }

    private static SystemActivityLogService CreateService(TestMocks mocks) => new(mocks.UnitOfWork.Object, new SystemActivityLogSearchRequestValidator());
    private static TestMocks CreateMocks()
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.SystemActivityLogs).Returns(mocks.Logs.Object);
        mocks.UnitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mocks;
    }
    private static SystemActivityLog BuildLog() => new() { SystemActivityLogId = 1, FunctionName = "Login", OperationType = "Login", ExecutionResult = "Success" };
    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<ISystemActivityLogRp> Logs { get; } = new();
    }
}
