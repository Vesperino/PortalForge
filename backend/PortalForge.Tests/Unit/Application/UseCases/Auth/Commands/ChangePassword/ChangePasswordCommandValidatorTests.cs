using FluentAssertions;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Auth.Commands.ChangePassword;
using PortalForge.Application.UseCases.Auth.Commands.ChangePassword.Validation;
using Xunit;

namespace PortalForge.Tests.Unit.Application.UseCases.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ChangePasswordCommandValidator _validator;

    public ChangePasswordCommandValidatorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validator = new ChangePasswordCommandValidator(_unitOfWorkMock.Object);
        SetupDefaultMocks();
    }

    private void SetupDefaultMocks()
    {
        // Setup default mocks for empty/invalid values that trigger async validation
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Entities.User?)null);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        // Override default mock for valid case
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(new Domain.Entities.User { Id = userId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_EmptyUserId_FailsValidation()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            UserId = Guid.Empty,
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(Guid.Empty))
            .ReturnsAsync((Domain.Entities.User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_EmptyCurrentPassword_FailsValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = string.Empty,
            NewPassword = "NewPassword123!"
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(new Domain.Entities.User { Id = userId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CurrentPassword");
    }

    [Fact]
    public async Task Validate_EmptyNewPassword_FailsValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "OldPassword123!",
            NewPassword = string.Empty
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(new Domain.Entities.User { Id = userId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public async Task Validate_NewPasswordTooShort_FailsValidation(string newPassword)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "OldPassword123!",
            NewPassword = newPassword
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(new Domain.Entities.User { Id = userId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "NewPassword" &&
            e.ErrorMessage.Contains("at least 8 characters"));
    }

    [Theory]
    [InlineData("nouppercasepassword123!")]
    [InlineData("NOLOWERCASEPASSWORD123!")]
    [InlineData("NoNumberPassword!")]
    [InlineData("NoSpecialChar123")]
    public async Task Validate_WeakNewPassword_FailsValidation(string newPassword)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "OldPassword123!",
            NewPassword = newPassword
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(new Domain.Entities.User { Id = userId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public async Task Validate_SameCurrentAndNewPassword_FailsValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "Password123!",
            NewPassword = "Password123!"
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(new Domain.Entities.User { Id = userId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "NewPassword" &&
            e.ErrorMessage.Contains("different from"));
    }

    [Fact]
    public async Task Validate_AllFieldsValid_PassesAllRules()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewSecure!Password456"
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(new Domain.Entities.User { Id = userId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
