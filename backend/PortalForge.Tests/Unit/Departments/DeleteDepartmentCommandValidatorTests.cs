using FluentValidation.TestHelper;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Departments.Commands.DeleteDepartment;
using PortalForge.Application.UseCases.Departments.Commands.DeleteDepartment.Validation;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Departments;

public class DeleteDepartmentCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly DeleteDepartmentCommandValidator _validator;

    public DeleteDepartmentCommandValidatorTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _validator = new DeleteDepartmentCommandValidator(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Validate_EmptyDepartmentId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new DeleteDepartmentCommand { DepartmentId = Guid.Empty };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DepartmentId)
            .WithErrorMessage("Department ID is required");
    }

    [Fact]
    public async Task Validate_DepartmentDoesNotExist_ShouldHaveValidationError()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        var command = new DeleteDepartmentCommand { DepartmentId = departmentId };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DepartmentId)
            .WithErrorMessage("Department does not exist");
    }

    [Fact]
    public async Task Validate_ValidCommand_ShouldNotHaveValidationError()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(new Department { Id = departmentId });

        var command = new DeleteDepartmentCommand { DepartmentId = departmentId };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
