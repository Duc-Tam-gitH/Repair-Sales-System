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
}
