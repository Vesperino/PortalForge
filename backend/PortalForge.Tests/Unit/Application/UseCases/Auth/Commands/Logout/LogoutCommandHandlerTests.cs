using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.Auth.Commands.Logout;

namespace PortalForge.Tests.Unit.Application.UseCases.Auth.Commands.Logout;

public class LogoutCommandHandlerTests
{
    private readonly Mock<ISupabaseAuthService> _authServiceMock;
    private readonly Mock<ILogger<LogoutCommandHandler>> _loggerMock;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _authServiceMock = new Mock<ISupabaseAuthService>();
        _loggerMock = new Mock<ILogger<LogoutCommandHandler>>();

        _handler = new LogoutCommandHandler(
            _authServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsLogoutAndReturnsUnit()
    {
        // Arrange
        var command = new LogoutCommand();

        _authServiceMock
            .Setup(x => x.LogoutAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);

        _authServiceMock.Verify(
            x => x.LogoutAsync(It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ServiceReturnsFalse_ThrowsException()
    {
        // Arrange
        var command = new LogoutCommand();

        _authServiceMock
            .Setup(x => x.LogoutAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("Logout failed. Please try again.");
    }
}
