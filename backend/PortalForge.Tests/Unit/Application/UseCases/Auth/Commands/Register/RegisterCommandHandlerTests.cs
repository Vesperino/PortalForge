using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Common.Models;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.Auth.Commands.Register;

namespace PortalForge.Tests.Unit.Application.UseCases.Auth.Commands.Register;

public class RegisterCommandHandlerTests
{
    private readonly Mock<ISupabaseAuthService> _authServiceMock;
    private readonly Mock<IUnifiedValidatorService> _validatorServiceMock;
    private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _authServiceMock = new Mock<ISupabaseAuthService>();
        _validatorServiceMock = new Mock<IUnifiedValidatorService>();
        _loggerMock = new Mock<ILogger<RegisterCommandHandler>>();

        _handler = new RegisterCommandHandler(
            _authServiceMock.Object,
            _validatorServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Test123!@#",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        var authResult = new AuthResult
        {
            Success = true,
            UserId = Guid.NewGuid(),
            Email = command.Email
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _authServiceMock
            .Setup(x => x.RegisterAsync(
                command.Email,
                command.Password,
                command.FirstName,
                command.LastName))
            .ReturnsAsync(authResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(authResult.UserId!.Value);
        result.Email.Should().Be(command.Email);
        result.Message.Should().Contain("Registration successful");

        _validatorServiceMock.Verify(
            x => x.ValidateAsync(command),
            Times.Once);

        _authServiceMock.Verify(
            x => x.RegisterAsync(
                command.Email,
                command.Password,
                command.FirstName,
                command.LastName),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RegistrationFails_ThrowsValidationException()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Test123!@#",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        var authResult = new AuthResult
        {
            Success = false,
            ErrorMessage = "Email already exists"
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _authServiceMock
            .Setup(x => x.RegisterAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(authResult);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Registration failed");

        _authServiceMock.Verify(
            x => x.RegisterAsync(
                command.Email,
                command.Password,
                command.FirstName,
                command.LastName),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidatorThrowsException_PropagatesException()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "invalid-email",
            Password = "weak",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .ThrowsAsync(new ValidationException("Validation failed"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        _authServiceMock.Verify(
            x => x.RegisterAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SuccessfulRegistration_LogsInformation()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Test123!@#",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        var userId = Guid.NewGuid();
        var authResult = new AuthResult
        {
            Success = true,
            UserId = userId,
            Email = command.Email
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _authServiceMock
            .Setup(x => x.RegisterAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(authResult);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registering new user")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User registered successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
