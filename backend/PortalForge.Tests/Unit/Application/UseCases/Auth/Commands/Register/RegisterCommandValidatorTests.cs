using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Auth.Commands.Register.Validation;
using PortalForge.Domain.Entities;
using RegisterCommand = PortalForge.Application.UseCases.Auth.Commands.Register.RegisterCommand;

namespace PortalForge.Tests.Unit.Application.UseCases.Auth.Commands.Register;

public class RegisterCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _unitOfWorkMock
            .Setup(x => x.UserRepository)
            .Returns(_userRepositoryMock.Object);

        _validator = new RegisterCommandValidator(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Validator_ValidCommand_PassesValidation()
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

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    public async Task Validator_EmptyEmail_FailsValidation(string email)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = email!,
            Password = "Test123!@#",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public async Task Validator_InvalidEmailFormat_FailsValidation(string email)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = email,
            Password = "Test123!@#",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public async Task Validator_EmailAlreadyExists_FailsValidation()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "existing@example.com",
            Password = "Test123!@#",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            FirstName = "Jane",
            LastName = "Smith",
            Department = "HR",
            Position = "Manager",
            IsEmailVerified = true  // User with verified email should block registration
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email already exists");
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("1234567")]
    public async Task Validator_PasswordTooShort_FailsValidation(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("password123!")]
    [InlineData("alllowercase1!")]
    public async Task Validator_PasswordNoUppercase_FailsValidation(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter");
    }

    [Theory]
    [InlineData("PASSWORD123!")]
    [InlineData("ALLUPPERCASE1!")]
    public async Task Validator_PasswordNoLowercase_FailsValidation(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter");
    }

    [Theory]
    [InlineData("Password!")]
    [InlineData("NoNumbers!")]
    public async Task Validator_PasswordNoNumber_FailsValidation(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one number");
    }

    [Theory]
    [InlineData("Password123")]
    [InlineData("NoSpecial1")]
    public async Task Validator_PasswordNoSpecialCharacter_FailsValidation(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one special character");
    }

    [Theory]
    [InlineData("")]
    public async Task Validator_EmptyFirstName_FailsValidation(string firstName)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Test123!@#",
            FirstName = firstName!,
            LastName = "Doe",
            Department = "IT",
            Position = "Developer"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required");
    }

    [Theory]
    [InlineData("")]
    public async Task Validator_EmptyLastName_FailsValidation(string lastName)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Test123!@#",
            FirstName = "John",
            LastName = lastName!,
            Department = "IT",
            Position = "Developer"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required");
    }

    [Theory]
    [InlineData("")]
    public async Task Validator_EmptyDepartment_FailsValidation(string department)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Test123!@#",
            FirstName = "John",
            LastName = "Doe",
            Department = department!,
            Position = "Developer"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Department)
            .WithErrorMessage("Department is required");
    }

    [Theory]
    [InlineData("")]
    public async Task Validator_EmptyPosition_FailsValidation(string position)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Test123!@#",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = position!
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorMessage("Position is required");
    }
}
