using FluentAssertions;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Admin.Commands.UpdateUser;
using PortalForge.Application.UseCases.Admin.Commands.UpdateUser.Validation;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Application.UseCases.Admin.Commands.UpdateUser;

public class UpdateUserCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateUserCommandValidator _validator;

    public UpdateUserCommandValidatorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validator = new UpdateUserCommandValidator(_unitOfWorkMock.Object);
        SetupDefaultMocks();
    }

    private void SetupDefaultMocks()
    {
        // Setup default mocks for empty/invalid values that trigger async validation
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Department?)null);

        _unitOfWorkMock.Setup(x => x.PositionRepository.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Position?)null);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var positionId = Guid.NewGuid();

        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT Department",
            DepartmentId = departmentId,
            Position = "Software Engineer",
            PositionId = positionId,
            PhoneNumber = "+48123456789",
            Role = "Employee",
            IsActive = true,
            UpdatedBy = Guid.NewGuid()
        };

        // Override default mocks for valid case
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(new Department { Id = departmentId, Name = "IT" });

        _unitOfWorkMock.Setup(x => x.PositionRepository.GetByIdAsync(positionId))
            .ReturnsAsync(new Position { Id = positionId, Name = "Software Engineer" });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // UserId validation tests
    [Fact]
    public async Task Validate_EmptyUserId_FailsValidation()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            UserId = Guid.Empty,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "UserId" &&
            e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_UserDoesNotExist_FailsValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "UserId" &&
            e.ErrorMessage.Contains("does not exist"));
    }

    // FirstName validation tests
    [Fact]
    public async Task Validate_EmptyFirstName_FailsValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = string.Empty,
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = firstName,
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = new string('a', 101),
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = string.Empty,
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = lastName,
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = new string('a', 101),
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = string.Empty,
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = new string('a', 201),
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            DepartmentId = departmentId,
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            DepartmentId = null,
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = string.Empty,
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = new string('a', 201),
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Position" &&
            e.ErrorMessage.Contains("200 characters"));
    }

    // PositionId validation tests
    [Fact]
    public async Task Validate_PositionIdDoesNotExist_FailsValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var positionId = Guid.NewGuid();

        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            PositionId = positionId,
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        _unitOfWorkMock.Setup(x => x.PositionRepository.GetByIdAsync(positionId))
            .ReturnsAsync((Position?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            (e.PropertyName == "PositionId" || e.PropertyName.StartsWith("PositionId")) &&
            e.ErrorMessage.Contains("does not exist"));
    }

    [Fact]
    public async Task Validate_NullPositionId_PassesValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            PositionId = null,
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // PhoneNumber validation tests
    [Theory]
    [InlineData("+48123456789")]
    [InlineData("+1234567890")]
    [InlineData("+999999999999999")]
    public async Task Validate_ValidPhoneNumber_PassesValidation(string phoneNumber)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            PhoneNumber = phoneNumber,
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            PhoneNumber = phoneNumber,
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            PhoneNumber = null,
            Role = "Employee",
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = string.Empty,
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = role,
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

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
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = role,
            UpdatedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // UpdatedBy validation tests
    [Fact]
    public async Task Validate_EmptyUpdatedBy_FailsValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            Position = "Developer",
            Role = "Employee",
            UpdatedBy = Guid.Empty
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "UpdatedBy" &&
            e.ErrorMessage.Contains("required"));
    }
}
