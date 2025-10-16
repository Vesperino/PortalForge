using FluentAssertions;
using FluentValidation.TestHelper;
using PortalForge.Application.UseCases.Auth.Commands.Login;
using PortalForge.Application.UseCases.Auth.Commands.Login.Validation;

namespace PortalForge.Tests.Unit.Application.UseCases.Auth.Commands.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEmail_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = string.Empty,
            Password = "Password123!"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email jest wymagany");
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "invalid-email",
            Password = "Password123!"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Nieprawidłowy format email");
    }

    [Fact]
    public void Validate_EmptyPassword_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = string.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Hasło jest wymagane");
    }

    [Fact]
    public void Validate_ShortPassword_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Short1!"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Hasło musi mieć minimum 8 znaków");
    }
}
