using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.Departments.Commands.DeleteDepartment;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Departments;

public class DeleteDepartmentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<DeleteDepartmentCommandHandler>> _mockLogger;
    private readonly DeleteDepartmentCommandHandler _handler;

    public DeleteDepartmentCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<DeleteDepartmentCommandHandler>>();
        _handler = new DeleteDepartmentCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_SoftDeletesDepartment()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var existingDepartment = new Department
        {
            Id = departmentId,
            Name = "Test Department",
            IsActive = true
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(existingDepartment);

        var command = new DeleteDepartmentCommand { DepartmentId = departmentId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(MediatR.Unit.Value, result);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.DeleteAsync(departmentId), Times.Once);
    }

    [Fact]
    public async Task Handle_DepartmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        var command = new DeleteDepartmentCommand { DepartmentId = departmentId };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DepartmentWithEmployees_UnassignsAllEmployees()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var employee1 = new User { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", DepartmentId = departmentId };
        var employee2 = new User { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", DepartmentId = departmentId };

        var existingDepartment = new Department
        {
            Id = departmentId,
            Name = "Test Department",
            IsActive = true,
            Employees = new List<User> { employee1, employee2 }
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(existingDepartment);

        _mockUnitOfWork.Setup(u => u.UserRepository.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteDepartmentCommand { DepartmentId = departmentId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(MediatR.Unit.Value, result);
        Assert.Null(employee1.DepartmentId);
        Assert.Null(employee2.DepartmentId);
        _mockUnitOfWork.Verify(u => u.UserRepository.UpdateAsync(employee1, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.UserRepository.UpdateAsync(employee2, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.DeleteAsync(departmentId), Times.Once);
    }

    [Fact]
    public async Task Handle_DepartmentWithSubdepartments_PromotesSubdepartmentsToRootLevel()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var subDept1 = new Department { Id = Guid.NewGuid(), Name = "Sub Department 1", ParentDepartmentId = departmentId };
        var subDept2 = new Department { Id = Guid.NewGuid(), Name = "Sub Department 2", ParentDepartmentId = departmentId };

        var existingDepartment = new Department
        {
            Id = departmentId,
            Name = "Test Department",
            IsActive = true,
            ChildDepartments = new List<Department> { subDept1, subDept2 }
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(existingDepartment);

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.UpdateAsync(It.IsAny<Department>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteDepartmentCommand { DepartmentId = departmentId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(MediatR.Unit.Value, result);
        Assert.Null(subDept1.ParentDepartmentId);
        Assert.Null(subDept2.ParentDepartmentId);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.UpdateAsync(subDept1), Times.Once);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.UpdateAsync(subDept2), Times.Once);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.DeleteAsync(departmentId), Times.Once);
    }

    [Fact]
    public async Task Handle_DepartmentWithEmployeesAndSubdepartments_UnassignsEmployeesAndPromotesSubdepartments()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var employee1 = new User { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", DepartmentId = departmentId };
        var employee2 = new User { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", DepartmentId = departmentId };
        var subDept1 = new Department { Id = Guid.NewGuid(), Name = "Sub Department 1", ParentDepartmentId = departmentId };
        var subDept2 = new Department { Id = Guid.NewGuid(), Name = "Sub Department 2", ParentDepartmentId = departmentId };

        var existingDepartment = new Department
        {
            Id = departmentId,
            Name = "Test Department",
            IsActive = true,
            Employees = new List<User> { employee1, employee2 },
            ChildDepartments = new List<Department> { subDept1, subDept2 }
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(existingDepartment);

        _mockUnitOfWork.Setup(u => u.UserRepository.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.UpdateAsync(It.IsAny<Department>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteDepartmentCommand { DepartmentId = departmentId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(MediatR.Unit.Value, result);

        // Verify employees unassigned
        Assert.Null(employee1.DepartmentId);
        Assert.Null(employee2.DepartmentId);
        _mockUnitOfWork.Verify(u => u.UserRepository.UpdateAsync(employee1, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.UserRepository.UpdateAsync(employee2, It.IsAny<CancellationToken>()), Times.Once);

        // Verify subdepartments promoted to root
        Assert.Null(subDept1.ParentDepartmentId);
        Assert.Null(subDept2.ParentDepartmentId);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.UpdateAsync(subDept1), Times.Once);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.UpdateAsync(subDept2), Times.Once);

        // Verify department soft deleted
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.DeleteAsync(departmentId), Times.Once);
    }

    [Fact]
    public async Task Handle_DepartmentWithoutEmployeesOrSubdepartments_OnlySoftDeletesDepartment()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var existingDepartment = new Department
        {
            Id = departmentId,
            Name = "Test Department",
            IsActive = true,
            Employees = new List<User>(),
            ChildDepartments = new List<Department>()
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(existingDepartment);

        var command = new DeleteDepartmentCommand { DepartmentId = departmentId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(MediatR.Unit.Value, result);
        _mockUnitOfWork.Verify(u => u.UserRepository.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.UpdateAsync(It.IsAny<Department>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.DeleteAsync(departmentId), Times.Once);
    }
}
