using System.Text.Json;
using EnterpriseUserManagement.Api.Middleware;
using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EnterpriseUserManagement.UnitTests.Middleware;

public class GlobalExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlingMiddleware>> _loggerMock = new();

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNextDelegate()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenDomainValidationException_Returns400BadRequest()
    {
        // Arrange
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new DomainValidationException("Invalid input provided.");
        var middleware = new GlobalExceptionHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        var response = await ReadResponseBodyAsync(context);
        Assert.False(response.Success);
        Assert.Equal("Invalid input provided.", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_WhenUserNotFoundException_Returns404NotFound()
    {
        // Arrange
        var context = CreateHttpContext();
        var userId = Guid.NewGuid();
        RequestDelegate next = _ => throw new UserNotFoundException(userId);
        var middleware = new GlobalExceptionHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        var response = await ReadResponseBodyAsync(context);
        Assert.False(response.Success);
        Assert.Contains(userId.ToString(), response.Message);
    }

    [Fact]
    public async Task InvokeAsync_WhenDuplicateEmailException_Returns409Conflict()
    {
        // Arrange
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new DuplicateEmailException("test@enterprise.com");
        var middleware = new GlobalExceptionHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        var response = await ReadResponseBodyAsync(context);
        Assert.False(response.Success);
        Assert.Contains("test@enterprise.com", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_WhenStorageCorruptedException_Returns500InternalServerError()
    {
        // Arrange
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new StorageCorruptedException("users.json", "Database connection failed unexpectedly.");
        var middleware = new GlobalExceptionHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var response = await ReadResponseBodyAsync(context);
        Assert.False(response.Success);
        Assert.Contains("Storage failure", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericException_Returns500InternalServerError()
    {
        // Arrange
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new InvalidOperationException("Something unexpected broke.");
        var middleware = new GlobalExceptionHandlingMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var response = await ReadResponseBodyAsync(context);
        Assert.False(response.Success);
        Assert.Equal("An unexpected server error occurred. Please try again later.", response.Message);
    }

    [Fact]
    public void UseGlobalExceptionHandling_ExtensionMethod_RegistersMiddleware()
    {
        // Arrange
        var appMock = new Mock<IApplicationBuilder>();
        appMock.Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appMock.Object);

        // Act
        var result = appMock.Object.UseGlobalExceptionHandling();

        // Assert
        Assert.NotNull(result);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ApiResponse<object>> ReadResponseBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ApiResponse<object>>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
}
