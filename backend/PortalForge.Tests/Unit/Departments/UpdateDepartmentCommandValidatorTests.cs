using FluentValidation.TestHelper;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Departments.Commands.UpdateDepartment;
using PortalForge.Application.UseCases.Departments.Commands.UpdateDepartment.Validation;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Departments;

public class UpdateDepartmentCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UpdateDepartmentCommandValidator _validator;

    public UpdateDepartmentCommandValidatorTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _validator = new UpdateDepartmentCommandValidator(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Validate_DepartmentDoesNotExist_ShouldHaveValidationError()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        var command = new UpdateDepartmentCommand
        {
            DepartmentId = departmentId,
            Name = "Valid Name"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DepartmentId)
            .WithErrorMessage("Department does not exist");
    }

    [Fact]
    public async Task Validate_EmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(new Department { Id = departmentId });

        var command = new UpdateDepartmentCommand
        {
            DepartmentId = departmentId,
            Name = ""
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Department name is required");
    }

    [Fact]
    public async Task Validate_SelfAsParent_ShouldHaveValidationError()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(new Department { Id = departmentId });

        var command = new UpdateDepartmentCommand
        {
            DepartmentId = departmentId,
            Name = "Valid Name",
            ParentDepartmentId = departmentId // Same as DepartmentId!
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ParentDepartmentId)
            .WithErrorMessage("Department cannot be its own parent");
    }

    [Fact]
    public async Task Validate_ParentDepartmentDoesNotExist_ShouldHaveValidationError()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(new Department { Id = departmentId });

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(parentId))
            .ReturnsAsync((Department?)null);

        var command = new UpdateDepartmentCommand
        {
            DepartmentId = departmentId,
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
        var departmentId = Guid.NewGuid();
        var headId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(new Department { Id = departmentId });

        _mockUnitOfWork.Setup(u => u.UserRepository.GetByIdAsync(headId))
            .ReturnsAsync((User?)null);

        var command = new UpdateDepartmentCommand
        {
            DepartmentId = departmentId,
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
        var departmentId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var headId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(new Department { Id = departmentId });

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(parentId))
            .ReturnsAsync(new Department { Id = parentId, IsActive = true });

        _mockUnitOfWork.Setup(u => u.UserRepository.GetByIdAsync(headId))
            .ReturnsAsync(new User { Id = headId, IsActive = true });

        var command = new UpdateDepartmentCommand
        {
            DepartmentId = departmentId,
            Name = "Valid Department",
            Description = "Valid Description",
            ParentDepartmentId = parentId,
            DepartmentHeadId = headId,
            IsActive = true
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
