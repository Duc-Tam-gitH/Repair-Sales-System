using FluentAssertions;
using FluentValidation;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.Notification;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Notification;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.NotificationTests;

public class NotificationTemplateServiceTests
{
    [Fact]
    public async Task UpdateAsync_ShouldSaveTemplateHistory_WhenVariablesAreValid()
    {
        var template = BuildTemplate();
        var mocks = CreateMocks(template);
        var service = CreateService(mocks);

        var response = await service.UpdateAsync(new UpdateNotificationTemplateRequest
        {
            TemplateCode = template.TemplateCode,
            Subject = "OTP for {full_name}",
            Content = "Code: {otp_code}",
            ActorUserId = 1,
            ActorRole = RoleConstants.Admin
        });

        response.Subject.Should().Be("OTP for {full_name}");
        template.Content.Should().Be("Code: {otp_code}");
        mocks.Histories.Verify(repo => repo.AddAsync(It.Is<NotificationTemplateHistory>(history => history.Action == "Update")), Times.Once);
        mocks.SystemActivityLogs.Verify(repo => repo.AddAsync(It.Is<SystemActivityLog>(log => log.FunctionName == "Manage Notification Templates")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowValidationException_WhenVariableIsNotAllowed()
    {
        var template = BuildTemplate();
        var service = CreateService(CreateMocks(template));

        var act = async () => await service.UpdateAsync(new UpdateNotificationTemplateRequest
        {
            TemplateCode = template.TemplateCode,
            Subject = "Invalid",
            Content = "Code: {bad_variable}",
            ActorUserId = 1,
            ActorRole = RoleConstants.Admin
        });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetTemplatesAsync_ShouldThrowUnauthorizedException_WhenActorIsNotAdmin()
    {
        var service = CreateService(CreateMocks(BuildTemplate()));

        var act = async () => await service.GetTemplatesAsync(RoleConstants.Manager);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only Administrator can manage notification templates.");
    }

    [Fact]
    public async Task PreviewAsync_ShouldReplaceVariablesWithSampleData()
    {
        var template = BuildTemplate();
        var service = CreateService(CreateMocks(template));

        var response = await service.PreviewAsync(new PreviewNotificationTemplateRequest
        {
            TemplateCode = template.TemplateCode,
            Subject = "Hello {full_name}",
            Content = "Code: {otp_code}",
            ActorRole = RoleConstants.Admin,
            SampleData = new Dictionary<string, string>
            {
                ["full_name"] = "Jane",
                ["otp_code"] = "123456"
            }
        });

        response.Subject.Should().Be("Hello Jane");
        response.Content.Should().Be("Code: 123456");
    }

    private static NotificationTemplateService CreateService(TestMocks mocks)
    {
        return new NotificationTemplateService(
            mocks.UnitOfWork.Object,
            new UpdateNotificationTemplateRequestValidator());
    }

    private static TestMocks CreateMocks(NotificationTemplate template)
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.NotificationTemplates).Returns(mocks.Templates.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.NotificationTemplateHistories).Returns(mocks.Histories.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.SystemActivityLogs).Returns(mocks.SystemActivityLogs.Object);
        mocks.UnitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mocks.Templates.Setup(repo => repo.GetByCodeAsync(template.TemplateCode)).ReturnsAsync(template);
        mocks.Templates.Setup(repo => repo.GetWithDefaultsAsync()).ReturnsAsync(new[] { template });
        return mocks;
    }

    private static NotificationTemplate BuildTemplate() => new()
    {
        NotificationTemplateId = 10,
        TemplateCode = "SEND_OTP",
        TemplateType = "Email",
        TemplateName = "Send OTP",
        Subject = "OTP",
        Content = "Hello {full_name}, {otp_code}",
        DefaultSubject = "OTP",
        DefaultContent = "Hello {full_name}, {otp_code}",
        AllowedVariables = "{full_name},{otp_code},{creation_date}"
    };

    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<INotificationTemplateRp> Templates { get; } = new();
        public Mock<INotificationTemplateHistoryRp> Histories { get; } = new();
        public Mock<ISystemActivityLogRp> SystemActivityLogs { get; } = new();
    }
}
