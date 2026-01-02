using FluentValidation.TestHelper;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Departments.Commands.CreateDepartment;
using PortalForge.Application.UseCases.Departments.Commands.CreateDepartment.Validation;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Departments;

public class CreateDepartmentCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CreateDepartmentCommandValidator _validator;

    public CreateDepartmentCommandValidatorTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _validator = new CreateDepartmentCommandValidator(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Validate_EmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDepartmentCommand { Name = "" };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Department name is required");
    }

    [Fact]
    public async Task Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDepartmentCommand { Name = new string('a', 201) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Department name cannot exceed 200 characters");
    }

    [Fact]
    public async Task Validate_DescriptionTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDepartmentCommand
        {
            Name = "Valid Name",
            Description = new string('a', 1001)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 1000 characters");
    }

    [Fact]
    public async Task Validate_ParentDepartmentDoesNotExist_ShouldHaveValidationError()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(parentId))
            .ReturnsAsync((Department?)null);

        var command = new CreateDepartmentCommand
        {
            Name = "Valid Name",
            ParentDepartmentId = parentId
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ParentDepartmentId)
            .WithErrorMessage("Parent department does not exist");
    }

    [Fact]
    public async Task Validate_ParentDepartmentInactive_ShouldHaveValidationError()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(parentId))
            .ReturnsAsync(new Department { Id = parentId, IsActive = false });

        var command = new CreateDepartmentCommand
        {
            Name = "Valid Name",
            ParentDepartmentId = parentId
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ParentDepartmentId)
            .WithErrorMessage("Parent department does not exist");
    }

    [Fact]
    public async Task Validate_DepartmentHeadDoesNotExist_ShouldHaveValidationError()
    {
        // Arrange
        var headId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.UserRepository.GetByIdAsync(headId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new CreateDepartmentCommand
        {
            Name = "Valid Name",
            DepartmentHeadId = headId
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DepartmentHeadId)
            .WithErrorMessage("Department head user does not exist");
    }

    [Fact]
    public async Task Validate_ValidCommand_ShouldNotHaveValidationError()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var headId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(parentId))
            .ReturnsAsync(new Department { Id = parentId, IsActive = true });

        _mockUnitOfWork.Setup(u => u.UserRepository.GetByIdAsync(headId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = headId, IsActive = true });

        var command = new CreateDepartmentCommand
        {
            Name = "Valid Department",
            Description = "Valid Description",
            ParentDepartmentId = parentId,
            DepartmentHeadId = headId
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
