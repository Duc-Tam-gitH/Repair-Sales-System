using FluentAssertions;
using R_SS.BLL.DTOs.Authentication;
using R_SS.BLL.Validators.Authentication;

namespace R_SS.Tests.AuthTests;

public class RegisterRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ShouldPass_WhenAllRegisterFieldsAreValid()
    {
        var validator = new RegisterRequestValidator();

        var result = await validator.ValidateAsync(new RegisterRequest
        {
            Username = "john.doe",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Email = "john.doe@example.com",
            FullName = "John Doe",
            Phone = "0123456789",
            Address = "123 Main Street"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenUsernameIsTooShort()
    {
        var validator = new RegisterRequestValidator();

        var result = await validator.ValidateAsync(new RegisterRequest
        {
            Username = "abc",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Email = "john.doe@example.com",
            FullName = "John Doe"
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.Username));
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenConfirmPasswordDoesNotMatch()
    {
        var validator = new RegisterRequestValidator();

        var result = await validator.ValidateAsync(new RegisterRequest
        {
            Username = "john.doe",
            Password = "Password123!",
            ConfirmPassword = "Different123!",
            Email = "john.doe@example.com",
            FullName = "John Doe"
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.ConfirmPassword));
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_ForAllRequiredUc01InvalidFields()
    {
        var validator = new RegisterRequestValidator();

        var result = await validator.ValidateAsync(new RegisterRequest
        {
            Username = string.Empty,
            Password = "123",
            ConfirmPassword = "456",
            Email = "abc",
            FullName = string.Empty,
            Phone = new string('1', 21),
            Address = new string('A', 256)
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.Username));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.Password));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.ConfirmPassword));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.Email));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.FullName));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.Phone));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.Address));
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenMaximumLengthsAreExceeded()
    {
        var validator = new RegisterRequestValidator();

        var result = await validator.ValidateAsync(new RegisterRequest
        {
            Username = new string('u', 51),
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Email = "john.doe@example.com",
            FullName = new string('F', 101),
            Phone = new string('1', 21),
            Address = new string('A', 256)
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.Username));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.FullName));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.Phone));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterRequest.Address));
    }
}
