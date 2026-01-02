using FluentAssertions;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Admin.Commands.CreateUser;
using PortalForge.Application.UseCases.Admin.Commands.CreateUser.Validation;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Application.UseCases.Admin.Commands.CreateUser;

public class CreateUserCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateUserCommandValidator _validator;

    public CreateUserCommandValidatorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validator = new CreateUserCommandValidator(_unitOfWorkMock.Object);
        SetupDefaultMocks();
    }

    private void SetupDefaultMocks()
    {
        // Setup default mocks for empty/invalid values that trigger async validation
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Department?)null);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var command = new CreateUserCommand
        {
            Email = "john.doe@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT Department",
            DepartmentId = departmentId,
            Position = "Software Engineer",
            PhoneNumber = "+48123456789",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        // Override default mocks for valid case
        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(new Department { Id = departmentId, Name = "IT" });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Email validation tests
    [Fact]
    public async Task Validate_EmptyEmail_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = string.Empty,
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Email" &&
            e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public async Task Validate_InvalidEmailFormat_FailsValidation(string email)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = email,
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Email" &&
            e.ErrorMessage.Contains("Invalid email format"));
    }

    [Fact]
    public async Task Validate_EmailTooLong_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = new string('a', 247) + "@test.com", // 256 characters
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Email" &&
            e.ErrorMessage.Contains("255 characters"));
    }

    [Fact]
    public async Task Validate_DuplicateEmail_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "existing@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        // Override default mock to return existing user
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = command.Email });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Email" &&
            e.ErrorMessage.Contains("already exists"));
    }

    // Password validation tests
    [Fact]
    public async Task Validate_EmptyPassword_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = string.Empty,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Password" &&
            e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public async Task Validate_PasswordTooShort_FailsValidation(string password)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Password" &&
            e.ErrorMessage.Contains("at least 8 characters"));
    }

    [Fact]
    public async Task Validate_PasswordTooLong_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = new string('a', 101), // 101 characters
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Password" &&
            e.ErrorMessage.Contains("100 characters"));
    }

    [Theory]
    [InlineData("nouppercase123!")]
    [InlineData("NOLOWERCASE123!")]
    [InlineData("NoNumbersHere!")]
    [InlineData("NoSpecialChar123")]
    public async Task Validate_WeakPassword_FailsValidation(string password)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    // FirstName validation tests
    [Fact]
    public async Task Validate_EmptyFirstName_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = string.Empty,
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "FirstName" &&
            e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("J")]
    [InlineData("A")]
    public async Task Validate_FirstNameTooShort_FailsValidation(string firstName)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = firstName,
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "FirstName" &&
            e.ErrorMessage.Contains("at least 2 characters"));
    }

    [Fact]
    public async Task Validate_FirstNameTooLong_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = new string('a', 101),
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "FirstName" &&
            e.ErrorMessage.Contains("100 characters"));
    }

    // LastName validation tests
    [Fact]
    public async Task Validate_EmptyLastName_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = string.Empty,
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "LastName" &&
            e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("D")]
    [InlineData("X")]
    public async Task Validate_LastNameTooShort_FailsValidation(string lastName)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = lastName,
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "LastName" &&
            e.ErrorMessage.Contains("at least 2 characters"));
    }

    [Fact]
    public async Task Validate_LastNameTooLong_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = new string('a', 101),
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "LastName" &&
            e.ErrorMessage.Contains("100 characters"));
    }

    // Department validation tests
    [Fact]
    public async Task Validate_EmptyDepartment_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = string.Empty,
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Department" &&
            e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_DepartmentTooLong_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = new string('a', 201),
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Department" &&
            e.ErrorMessage.Contains("200 characters"));
    }

    // DepartmentId validation tests
    [Fact]
    public async Task Validate_DepartmentIdDoesNotExist_FailsValidation()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            DepartmentId = departmentId,
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            (e.PropertyName == "DepartmentId" || e.PropertyName.StartsWith("DepartmentId")) &&
            e.ErrorMessage.Contains("does not exist"));
    }

    [Fact]
    public async Task Validate_NullDepartmentId_PassesValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            DepartmentId = null,
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // Position validation tests
    [Fact]
    public async Task Validate_EmptyPosition_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = string.Empty,
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Position" &&
            e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_PositionTooLong_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = new string('a', 201),
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Position" &&
            e.ErrorMessage.Contains("200 characters"));
    }

    // PhoneNumber validation tests
    [Theory]
    [InlineData("+48123456789")]
    [InlineData("+1234567890")]
    [InlineData("+999999999999999")]
    public async Task Validate_ValidPhoneNumber_PassesValidation(string phoneNumber)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            PhoneNumber = phoneNumber,
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("+0123456789")]
    [InlineData("abc123")]
    [InlineData("123-456-7890")]
    public async Task Validate_InvalidPhoneNumber_FailsValidation(string phoneNumber)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            PhoneNumber = phoneNumber,
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "PhoneNumber" &&
            e.ErrorMessage.Contains("Invalid phone number format"));
    }

    [Fact]
    public async Task Validate_NullPhoneNumber_PassesValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            PhoneNumber = null,
            Role = "Employee",
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // Role validation tests
    [Fact]
    public async Task Validate_EmptyRole_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = string.Empty,
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Role" &&
            e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("InvalidRole")]
    [InlineData("SuperAdmin")]
    [InlineData("Guest")]
    public async Task Validate_InvalidRole_FailsValidation(string role)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = role,
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Role" &&
            e.ErrorMessage.Contains("Admin, HR, Manager, Marketing, Employee"));
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("HR")]
    [InlineData("Manager")]
    [InlineData("Marketing")]
    [InlineData("Employee")]
    [InlineData("admin")] // Case-insensitive
    [InlineData("EMPLOYEE")] // Case-insensitive
    public async Task Validate_ValidRole_PassesValidation(string role)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = role,
            CreatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // CreatedBy validation tests
    [Fact]
    public async Task Validate_EmptyCreatedBy_FailsValidation()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "john@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            CreatedBy = Guid.Empty
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "CreatedBy" &&
            e.ErrorMessage.Contains("required"));
    }
}
