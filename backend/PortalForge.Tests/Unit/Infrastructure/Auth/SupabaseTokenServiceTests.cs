using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Infrastructure.Auth;
using Supabase;

namespace PortalForge.Tests.Unit.Infrastructure.Auth;

public class SupabaseTokenServiceTests
{
    private readonly Mock<ISupabaseClientFactory> _clientFactoryMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<SupabaseTokenService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly SupabaseTokenService _service;

    public SupabaseTokenServiceTests()
    {
        _clientFactoryMock = new Mock<ISupabaseClientFactory>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<SupabaseTokenService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var supabaseSettings = Options.Create(new SupabaseSettings
        {
            Url = "https://test.supabase.co",
            Key = "test-anon-key",
            ServiceRoleKey = "test-service-role-key"
        });

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://test.supabase.co")
        };

        _httpClientFactoryMock
            .Setup(x => x.CreateClient("Supabase"))
            .Returns(httpClient);

        _service = new SupabaseTokenService(
            supabaseSettings,
            _clientFactoryMock.Object,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        var responseBody = JsonSerializer.Serialize(new
        {
            access_token = "new-access-token",
            refresh_token = "new-refresh-token",
            expires_in = 3600
        });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        // Act
        var result = await _service.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddSeconds(3600), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithEmptyToken_ReturnsFailure()
    {
        // Arrange
        var refreshToken = "";

        // Act
        var result = await _service.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Refresh token is required");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithNullToken_ReturnsFailure()
    {
        // Act
        var result = await _service.RefreshTokenAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Refresh token is required");
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenApiReturnsError_ReturnsFailure()
    {
        // Arrange
        var refreshToken = "invalid-refresh-token";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("{\"error\": \"Invalid refresh token\"}")
            });

        // Act
        var result = await _service.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Token refresh failed");
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var refreshToken = "valid-token";
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
            () => _service.RefreshTokenAsync(refreshToken, cts.Token));
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenHttpError_ReturnsFailure()
    {
        // Arrange
        var refreshToken = "valid-token";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        // Act
        var result = await _service.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Unable to connect to authentication service");
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenInvalidJson_ReturnsFailure()
    {
        // Arrange
        var refreshToken = "valid-token";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("invalid json {{{")
            });

        // Act
        var result = await _service.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid response from authentication service");
    }
}
