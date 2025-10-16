using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Auth.Commands.ResetPassword;

namespace PortalForge.Tests.Unit.Application.UseCases.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<ISupabaseAuthService> _authServiceMock;
    private readonly Mock<IUnifiedValidatorService> _validatorServiceMock;
    private readonly Mock<ILogger<ResetPasswordCommandHandler>> _loggerMock;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _authServiceMock = new Mock<ISupabaseAuthService>();
        _validatorServiceMock = new Mock<IUnifiedValidatorService>();
        _loggerMock = new Mock<ILogger<ResetPasswordCommandHandler>>();

        _handler = new ResetPasswordCommandHandler(
            _authServiceMock.Object,
            _validatorServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidEmail_SendsResetEmail()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com"
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _authServiceMock
            .Setup(x => x.SendPasswordResetEmailAsync(command.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);

        _validatorServiceMock.Verify(
            x => x.ValidateAsync(command),
            Times.Once);

        _authServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(command.Email),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ServiceReturnsFalse_ThrowsException()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com"
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _authServiceMock
            .Setup(x => x.SendPasswordResetEmailAsync(command.Email))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Failed to send password reset email");
    }
}
