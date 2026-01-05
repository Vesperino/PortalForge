using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Infrastructure.Auth;

namespace PortalForge.Tests.Unit.Infrastructure.Auth;

public class SupabasePasswordServiceTests
{
    private readonly Mock<ISupabaseClientFactory> _clientFactoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<SupabasePasswordService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly SupabasePasswordService _service;

    public SupabasePasswordServiceTests()
    {
        _clientFactoryMock = new Mock<ISupabaseClientFactory>();
        _emailServiceMock = new Mock<IEmailService>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<SupabasePasswordService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var supabaseSettings = Options.Create(new SupabaseSettings
        {
            Url = "https://test.supabase.co",
            Key = "test-anon-key",
            ServiceRoleKey = "test-service-role-key"
        });

        var appSettings = Options.Create(new AppSettings
        {
            FrontendUrl = "https://app.example.com"
        });

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://test.supabase.co")
        };

        _httpClientFactoryMock
            .Setup(x => x.CreateClient("Supabase"))
            .Returns(httpClient);

        _httpClientFactoryMock
            .Setup(x => x.CreateClient("SupabaseAdmin"))
            .Returns(httpClient);

        _service = new SupabasePasswordService(
            supabaseSettings,
            appSettings,
            _clientFactoryMock.Object,
            _emailServiceMock.Object,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task UpdatePasswordAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var accessToken = "valid-access-token";
        var refreshToken = "valid-refresh-token";
        var newPassword = "NewPassword123!";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        // Act
        var result = await _service.UpdatePasswordAsync(accessToken, refreshToken, newPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePasswordAsync_WhenApiReturnsError_ReturnsFalse()
    {
        // Arrange
        var accessToken = "invalid-token";
        var refreshToken = "valid-refresh-token";
        var newPassword = "NewPassword123!";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("{\"error\": \"Invalid token\"}")
            });

        // Act
        var result = await _service.UpdatePasswordAsync(accessToken, refreshToken, newPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePasswordAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        // Act & Assert - TaskCanceledException inherits from OperationCanceledException
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _service.UpdatePasswordAsync("token", "refresh", "pass", cts.Token));
    }

    [Fact]
    public async Task UpdatePasswordAsync_WhenHttpError_ReturnsFalse()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        // Act
        var result = await _service.UpdatePasswordAsync("token", "refresh", "password");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AdminUpdatePasswordAsync_WithValidUserId_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newPassword = "NewPassword123!";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        // Act
        var result = await _service.AdminUpdatePasswordAsync(userId, newPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AdminUpdatePasswordAsync_WhenApiReturnsError_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newPassword = "NewPassword123!";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("{\"error\": \"User not found\"}")
            });

        // Act
        var result = await _service.AdminUpdatePasswordAsync(userId, newPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AdminUpdatePasswordAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        // Act & Assert - TaskCanceledException inherits from OperationCanceledException
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _service.AdminUpdatePasswordAsync(Guid.NewGuid(), "pass", cts.Token));
    }

    [Fact]
    public async Task AdminUpdatePasswordAsync_WhenHttpError_ReturnsFalse()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        // Act
        var result = await _service.AdminUpdatePasswordAsync(Guid.NewGuid(), "password");

        // Assert
        result.Should().BeFalse();
    }
}
