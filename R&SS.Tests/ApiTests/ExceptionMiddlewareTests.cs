using System.IO;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using R_SS.API.Middleware;
using R_SS.BLL.Exceptions;

namespace R_SS.Tests.ApiTests;

public class ExceptionMiddlewareTests
{
    [Theory]
    [MemberData(nameof(GetExceptionCases))]
    public async Task Invoke_ShouldMapExceptionsToExpectedStatusCodes(Exception exception, int expectedStatusCode)
    {
        var middleware = new ExceptionMiddleware(_ => throw exception);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.Invoke(context);

        context.Response.StatusCode.Should().Be(expectedStatusCode);
    }

    public static IEnumerable<object[]> GetExceptionCases()
    {
        yield return new object[] { new ValidationException(new[] { new ValidationFailure("Field", "Invalid value.") }), StatusCodes.Status400BadRequest };
        yield return new object[] { new ArgumentException("Bad request.") , StatusCodes.Status400BadRequest };
        yield return new object[] { new InvalidOperationException("Invalid operation."), StatusCodes.Status400BadRequest };
        yield return new object[] { new UnauthorizedAccessException("Unauthorized."), StatusCodes.Status401Unauthorized };
        yield return new object[] { new KeyNotFoundException("Missing key."), StatusCodes.Status404NotFound };
        yield return new object[] { new NotFoundException("Not found."), StatusCodes.Status404NotFound };
        yield return new object[] { new Exception("Unexpected."), StatusCodes.Status500InternalServerError };
    }
}
