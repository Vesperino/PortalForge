using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Common.Models;
using PortalForge.Application.UseCases.Auth.Commands.Login;

namespace PortalForge.Tests.Unit.Application.UseCases.Auth.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<ISupabaseAuthService> _authServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUnifiedValidatorService> _validatorServiceMock;
    private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _authServiceMock = new Mock<ISupabaseAuthService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorServiceMock = new Mock<IUnifiedValidatorService>();
        _loggerMock = new Mock<ILogger<LoginCommandHandler>>();

        _handler = new LoginCommandHandler(
            _authServiceMock.Object,
            _unitOfWorkMock.Object,
            _validatorServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResult()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var expectedAuthResult = new AuthResult
        {
            Success = true,
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _authServiceMock
            .Setup(x => x.LoginAsync(command.Email, command.Password))
            .ReturnsAsync(expectedAuthResult);

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByIdAsync(expectedAuthResult.UserId!.Value))
            .ReturnsAsync(new Domain.Entities.User
            {
                Id = expectedAuthResult.UserId.Value,
                Email = command.Email,
                FirstName = "Test",
                LastName = "User"
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");

        _validatorServiceMock.Verify(
            x => x.ValidateAsync(command),
            Times.Once);

        _authServiceMock.Verify(
            x => x.LoginAsync(command.Email, command.Password),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ThrowsException()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var failedResult = new AuthResult
        {
            Success = false,
            ErrorMessage = "Invalid credentials"
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _authServiceMock
            .Setup(x => x.LoginAsync(command.Email, command.Password))
            .ReturnsAsync(failedResult);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Invalid credentials");
    }
}
