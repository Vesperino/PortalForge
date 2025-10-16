using FluentAssertions;
using FluentValidation.TestHelper;
using PortalForge.Application.UseCases.Auth.Commands.ResetPassword;
using PortalForge.Application.UseCases.Auth.Commands.ResetPassword.Validation;

namespace PortalForge.Tests.Unit.Application.UseCases.Auth.Commands.ResetPassword;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator;

    public ResetPasswordCommandValidatorTests()
    {
        _validator = new ResetPasswordCommandValidator();
    }

    [Fact]
    public void Validate_ValidEmail_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com"
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
        var command = new ResetPasswordCommand
        {
            Email = string.Empty
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
        var command = new ResetPasswordCommand
        {
            Email = "invalid-email"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Nieprawidłowy format email");
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Email = new string('a', 250) + "@test.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email nie może przekraczać 255 znaków");
    }
}
