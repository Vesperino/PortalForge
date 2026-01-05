using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Auth;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Tests.Unit.Infrastructure.Auth;

public class SupabaseEmailVerificationServiceTests : IDisposable
{
    private readonly Mock<ISupabaseTokenService> _tokenServiceMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<SupabaseEmailVerificationService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly EmailVerificationTracker _verificationTracker;
    private readonly ApplicationDbContext _dbContext;
    private readonly SupabaseEmailVerificationService _service;

    public SupabaseEmailVerificationServiceTests()
    {
        _tokenServiceMock = new Mock<ISupabaseTokenService>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<SupabaseEmailVerificationService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _verificationTracker = new EmailVerificationTracker();

        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

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

        _service = new SupabaseEmailVerificationService(
            supabaseSettings,
            appSettings,
            _tokenServiceMock.Object,
            _dbContext,
            _httpClientFactoryMock.Object,
            _verificationTracker,
            _loggerMock.Object);
    }

    [Fact]
    public async Task VerifyEmailAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accessToken = "valid-access-token";

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsEmailVerified = false,
            Role = UserRole.Employee
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _tokenServiceMock
            .Setup(x => x.GetUserIdFromTokenAsync(accessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        // Act
        var result = await _service.VerifyEmailAsync(accessToken);

        // Assert
        result.Should().BeTrue();

        var updatedUser = await _dbContext.Users.FindAsync(userId);
        updatedUser!.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyEmailAsync_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var accessToken = "invalid-token";

        _tokenServiceMock
            .Setup(x => x.GetUserIdFromTokenAsync(accessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        // Act
        var result = await _service.VerifyEmailAsync(accessToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenUserNotFound_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accessToken = "valid-token";

        _tokenServiceMock
            .Setup(x => x.GetUserIdFromTokenAsync(accessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        // Act
        var result = await _service.VerifyEmailAsync(accessToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _tokenServiceMock
            .Setup(x => x.GetUserIdFromTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.VerifyEmailAsync("token", cts.Token));
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_WithValidEmail_ReturnsTrue()
    {
        // Arrange
        var email = "test@example.com";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "Test",
            LastName = "User",
            IsEmailVerified = false,
            Role = UserRole.Employee
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

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
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_WhenUserNotFound_ReturnsFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_WhenAlreadyVerified_ReturnsFalse()
    {
        // Arrange
        var email = "verified@example.com";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "Test",
            LastName = "User",
            IsEmailVerified = true,
            Role = UserRole.Employee
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_WhenRateLimited_ReturnsFalse()
    {
        // Arrange
        var email = "ratelimited@example.com";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "Test",
            LastName = "User",
            IsEmailVerified = false,
            Role = UserRole.Employee
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Record a resend to trigger rate limiting
        _verificationTracker.RecordResend(email);

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_WhenApiError_ReturnsFalse()
    {
        // Arrange
        var email = "apierror@example.com";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "Test",
            LastName = "User",
            IsEmailVerified = false,
            Role = UserRole.Employee
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.TooManyRequests,
                Content = new StringContent("{\"error\": \"Rate limited\"}")
            });

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var email = "cancelled@example.com";
        var cts = new CancellationTokenSource();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "Test",
            LastName = "User",
            IsEmailVerified = false,
            Role = UserRole.Employee
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.ResendVerificationEmailAsync(email, cts.Token));
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
