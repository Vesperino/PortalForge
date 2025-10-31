using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.Departments.Commands.UpdateDepartment;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Departments;

public class UpdateDepartmentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUnifiedValidatorService> _mockValidator;
    private readonly Mock<ILogger<UpdateDepartmentCommandHandler>> _mockLogger;
    private readonly UpdateDepartmentCommandHandler _handler;

    public UpdateDepartmentCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockValidator = new Mock<IUnifiedValidatorService>();
        _mockLogger = new Mock<ILogger<UpdateDepartmentCommandHandler>>();
        _handler = new UpdateDepartmentCommandHandler(
            _mockUnitOfWork.Object,
            _mockValidator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesDepartment()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var existingDepartment = new Department
        {
            Id = departmentId,
            Name = "Old Name",
            Description = "Old Description",
            IsActive = true
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(existingDepartment);

        var command = new UpdateDepartmentCommand
        {
            DepartmentId = departmentId,
            Name = "New Name",
            Description = "New Description",
            ParentDepartmentId = null,
            DepartmentHeadId = null,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(MediatR.Unit.Value, result);
        _mockValidator.Verify(v => v.ValidateAsync(command), Times.Once);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.UpdateAsync(
            It.Is<Department>(d =>
                d.Id == departmentId &&
                d.Name == "New Name" &&
                d.Description == "New Description" &&
                d.UpdatedAt != null
            )), Times.Once);
    }

    [Fact]
    public async Task Handle_DepartmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        var command = new UpdateDepartmentCommand
        {
            DepartmentId = departmentId,
            Name = "New Name"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UpdateParentDepartment_UpdatesParent()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var newParentId = Guid.NewGuid();

        var existingDepartment = new Department
        {
            Id = departmentId,
            Name = "Department",
            ParentDepartmentId = null,
            IsActive = true
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(existingDepartment);

        var command = new UpdateDepartmentCommand
        {
            DepartmentId = departmentId,
            Name = "Department",
            ParentDepartmentId = newParentId,
            IsActive = true
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.UpdateAsync(
            It.Is<Department>(d =>
                d.ParentDepartmentId == newParentId
            )), Times.Once);
    }

    [Fact]
    public async Task Handle_DeactivateDepartment_SetsIsActiveFalse()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        var existingDepartment = new Department
        {
            Id = departmentId,
            Name = "Department",
            IsActive = true
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(existingDepartment);

        var command = new UpdateDepartmentCommand
        {
            DepartmentId = departmentId,
            Name = "Department",
            IsActive = false
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.UpdateAsync(
            It.Is<Department>(d => d.IsActive == false)
        ), Times.Once);
    }
}
