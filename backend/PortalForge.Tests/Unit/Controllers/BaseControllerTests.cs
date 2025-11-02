using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;

namespace PortalForge.Tests.Unit.Controllers;

/// <summary>
/// Test controller that inherits from BaseController to test its protected methods.
/// </summary>
public class TestController : PortalForge.Api.Controllers.BaseController
{
    public bool TestTryGetUserId(out Guid userId) => TryGetUserId(out userId);

    public ActionResult? TestGetUserIdOrUnauthorized(out Guid userId, string? errorMessage = null) =>
        GetUserIdOrUnauthorized(out userId, errorMessage);

    public Guid? TestGetUserIdOrNull() => GetUserIdOrNull();
}

public class BaseControllerTests
{
    private readonly TestController _controller;

    public BaseControllerTests()
    {
        _controller = new TestController();
    }

    private void SetupUser(string? userId)
    {
        var claims = new List<Claim>();

        if (userId != null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    #region TryGetUserId Tests

    [Fact]
    public void TryGetUserId_ValidGuid_ReturnsTrueAndSetsUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        SetupUser(expectedUserId.ToString());

        // Act
        var result = _controller.TestTryGetUserId(out var userId);

        // Assert
        result.Should().BeTrue();
        userId.Should().Be(expectedUserId);
    }

    [Fact]
    public void TryGetUserId_NoUserIdClaim_ReturnsFalseAndSetsEmptyGuid()
    {
        // Arrange
        SetupUser(null);

        // Act
        var result = _controller.TestTryGetUserId(out var userId);

        // Assert
        result.Should().BeFalse();
        userId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TryGetUserId_EmptyString_ReturnsFalseAndSetsEmptyGuid()
    {
        // Arrange
        SetupUser(string.Empty);

        // Act
        var result = _controller.TestTryGetUserId(out var userId);

        // Assert
        result.Should().BeFalse();
        userId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TryGetUserId_InvalidGuid_ReturnsFalseAndSetsEmptyGuid()
    {
        // Arrange
        SetupUser("not-a-guid");

        // Act
        var result = _controller.TestTryGetUserId(out var userId);

        // Assert
        result.Should().BeFalse();
        userId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TryGetUserId_GuidWithUpperCase_ReturnsTrueAndParsesCorrectly()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        SetupUser(expectedUserId.ToString().ToUpper());

        // Act
        var result = _controller.TestTryGetUserId(out var userId);

        // Assert
        result.Should().BeTrue();
        userId.Should().Be(expectedUserId);
    }

    #endregion

    #region GetUserIdOrUnauthorized Tests

    [Fact]
    public void GetUserIdOrUnauthorized_ValidGuid_ReturnsNullAndSetsUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        SetupUser(expectedUserId.ToString());

        // Act
        var result = _controller.TestGetUserIdOrUnauthorized(out var userId);

        // Assert
        result.Should().BeNull();
        userId.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetUserIdOrUnauthorized_NoUserIdClaim_ReturnsUnauthorizedWithDefaultMessage()
    {
        // Arrange
        SetupUser(null);

        // Act
        var result = _controller.TestGetUserIdOrUnauthorized(out var userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<UnauthorizedObjectResult>();

        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be("User ID not found in token");
        userId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void GetUserIdOrUnauthorized_NoUserIdClaim_ReturnsUnauthorizedWithCustomMessage()
    {
        // Arrange
        SetupUser(null);
        const string customMessage = "Custom authentication error";

        // Act
        var result = _controller.TestGetUserIdOrUnauthorized(out var userId, customMessage);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<UnauthorizedObjectResult>();

        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be(customMessage);
        userId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void GetUserIdOrUnauthorized_InvalidGuid_ReturnsUnauthorized()
    {
        // Arrange
        SetupUser("invalid-guid-format");

        // Act
        var result = _controller.TestGetUserIdOrUnauthorized(out var userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<UnauthorizedObjectResult>();
        userId.Should().Be(Guid.Empty);
    }

    #endregion

    #region GetUserIdOrNull Tests

    [Fact]
    public void GetUserIdOrNull_ValidGuid_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        SetupUser(expectedUserId.ToString());

        // Act
        var result = _controller.TestGetUserIdOrNull();

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetUserIdOrNull_NoUserIdClaim_ReturnsNull()
    {
        // Arrange
        SetupUser(null);

        // Act
        var result = _controller.TestGetUserIdOrNull();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserIdOrNull_EmptyString_ReturnsNull()
    {
        // Arrange
        SetupUser(string.Empty);

        // Act
        var result = _controller.TestGetUserIdOrNull();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserIdOrNull_InvalidGuid_ReturnsNull()
    {
        // Arrange
        SetupUser("not-a-valid-guid");

        // Act
        var result = _controller.TestGetUserIdOrNull();

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
