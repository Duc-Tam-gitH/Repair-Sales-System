using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using R_SS.BLL.DTOs.Authentication;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Helpers;
using R_SS.BLL.Interfaces;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.Authentication;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.AuthTests;

public class AccountManagementServiceTests
{
    [Fact]
    public async Task ChangePasswordAsync_ShouldUpdateHashedPassword_WhenRequestIsValid()
    {
        var user = BuildUser();
        var userRepoMock = new Mock<IUserRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(userRepoMock);
        var passwordHasherMock = new Mock<IPasswordHasher>();

        userRepoMock.Setup(repo => repo.GetByIdAsync(user.UserId)).ReturnsAsync(user);
        passwordHasherMock.Setup(hasher => hasher.Verify("OldPassword123!", user.PasswordHash)).Returns(true);
        passwordHasherMock.Setup(hasher => hasher.Hash("NewPassword123!")).Returns("new-hash");

        var service = CreateService(unitOfWorkMock.Object, passwordHasherMock.Object);

        var response = await service.ChangePasswordAsync(new ChangePasswordRequest
        {
            UserId = user.UserId,
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        });

        response.Message.Should().Be("Password changed successfully.");
        user.PasswordHash.Should().Be("new-hash");
        userRepoMock.Verify(repo => repo.Update(user), Times.Once);
        unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldThrowUnauthorizedException_WhenCurrentPasswordIsWrong()
    {
        var user = BuildUser();
        var userRepoMock = new Mock<IUserRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(userRepoMock);
        var passwordHasherMock = new Mock<IPasswordHasher>();

        userRepoMock.Setup(repo => repo.GetByIdAsync(user.UserId)).ReturnsAsync(user);
        passwordHasherMock.Setup(hasher => hasher.Verify("WrongPassword123!", user.PasswordHash)).Returns(false);

        var service = CreateService(unitOfWorkMock.Object, passwordHasherMock.Object);

        var act = async () => await service.ChangePasswordAsync(new ChangePasswordRequest
        {
            UserId = user.UserId,
            CurrentPassword = "WrongPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        });

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Current password is incorrect.");
        userRepoMock.Verify(repo => repo.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldThrowValidationException_WhenPasswordsDoNotMatch()
    {
        var service = CreateService(new Mock<IUnitOfWork>().Object, new Mock<IPasswordHasher>().Object);

        var act = async () => await service.ChangePasswordAsync(new ChangePasswordRequest
        {
            UserId = 1,
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "Different123!"
        });

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(error => error.PropertyName == nameof(ChangePasswordRequest.ConfirmNewPassword));
    }

    [Fact]
    public async Task UpdatePersonalInfoAsync_ShouldUpdateCurrentUser_WhenRequestIsValid()
    {
        var user = BuildUser();
        var userRepoMock = new Mock<IUserRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(userRepoMock);
        var service = CreateService(unitOfWorkMock.Object, new Mock<IPasswordHasher>().Object);

        userRepoMock.Setup(repo => repo.GetByIdAsync(user.UserId)).ReturnsAsync(user);
        userRepoMock.Setup(repo => repo.ExistsEmailAsync("john.new@example.com")).ReturnsAsync(false);

        var response = await service.UpdatePersonalInfoAsync(new UpdatePersonalInfoRequest
        {
            UserId = user.UserId,
            FullName = "John New",
            Email = "john.new@example.com",
            Phone = "0987654321",
            Address = "456 New Street"
        });

        response.Message.Should().Be("Personal information updated successfully.");
        user.FullName.Should().Be("John New");
        user.Email.Should().Be("john.new@example.com");
        userRepoMock.Verify(repo => repo.Update(user), Times.Once);
        unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePersonalInfoAsync_ShouldThrowValidationException_WhenEmailIsInvalid()
    {
        var service = CreateService(new Mock<IUnitOfWork>().Object, new Mock<IPasswordHasher>().Object);

        var act = async () => await service.UpdatePersonalInfoAsync(new UpdatePersonalInfoRequest
        {
            UserId = 1,
            FullName = "John Doe",
            Email = "bad-email"
        });

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(error => error.PropertyName == nameof(UpdatePersonalInfoRequest.Email));
    }

    [Fact]
    public async Task UpdatePersonalInfoAsync_ShouldThrowInvalidOperationException_WhenEmailBelongsToAnotherUser()
    {
        var user = BuildUser();
        var userRepoMock = new Mock<IUserRp>();
        var unitOfWorkMock = CreateUnitOfWorkMock(userRepoMock);
        var service = CreateService(unitOfWorkMock.Object, new Mock<IPasswordHasher>().Object);

        userRepoMock.Setup(repo => repo.GetByIdAsync(user.UserId)).ReturnsAsync(user);
        userRepoMock.Setup(repo => repo.ExistsEmailAsync("taken@example.com")).ReturnsAsync(true);

        var act = async () => await service.UpdatePersonalInfoAsync(new UpdatePersonalInfoRequest
        {
            UserId = user.UserId,
            FullName = "John Doe",
            Email = "taken@example.com"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already exists.");
        userRepoMock.Verify(repo => repo.Update(It.IsAny<User>()), Times.Never);
    }

    private static AuthService CreateService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        return new AuthService(
            unitOfWork,
            passwordHasher,
            Mock.Of<IJwtTokenGenerator>(),
            Mock.Of<IOtpGenerator>(),
            Mock.Of<IEmailSender>(),
            new RegisterRequestValidator(),
            new LoginRequestValidator(),
            new ForgotPasswordRequestValidator(),
            new VerifyForgotPasswordOtpRequestValidator(),
            new ResetPasswordRequestValidator(),
            new ChangePasswordRequestValidator(),
            new UpdatePersonalInfoRequestValidator(),
            Mock.Of<ILogger<AuthService>>());
    }

    private static Mock<IUnitOfWork> CreateUnitOfWorkMock(Mock<IUserRp> userRepoMock)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(unit => unit.Users).Returns(userRepoMock.Object);
        unitOfWorkMock.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return unitOfWorkMock;
    }

    private static User BuildUser()
    {
        return new User
        {
            UserId = 1,
            Username = "john.doe",
            PasswordHash = "old-hash",
            Email = "john.doe@example.com",
            FullName = "John Doe",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
