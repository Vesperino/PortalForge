using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.Departments.Queries.GetDepartmentEmployees;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Departments;

public class GetDepartmentEmployeesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<GetDepartmentEmployeesQueryHandler>> _mockLogger;
    private readonly GetDepartmentEmployeesQueryHandler _handler;

    public GetDepartmentEmployeesQueryHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<GetDepartmentEmployeesQueryHandler>>();
        _handler = new GetDepartmentEmployeesQueryHandler(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_DepartmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        var query = new GetDepartmentEmployeesQuery { DepartmentId = departmentId };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DepartmentWithNoEmployees_ReturnsEmptyList()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        var department = new Department
        {
            Id = departmentId,
            Name = "Empty Department",
            IsActive = true,
            Employees = new List<User>()
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(department);

        var query = new GetDepartmentEmployeesQuery { DepartmentId = departmentId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_DepartmentWithActiveEmployees_ReturnsEmployeeList()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        var department = new Department
        {
            Id = departmentId,
            Name = "Engineering",
            IsActive = true,
            Employees = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    Position = "Developer",
                    DepartmentId = departmentId,
                    IsActive = true
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    Position = "Designer",
                    DepartmentId = departmentId,
                    IsActive = true
                }
            }
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(department);

        var query = new GetDepartmentEmployeesQuery { DepartmentId = departmentId, IncludeInactive = false };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.FirstName == "John" && e.LastName == "Doe");
        Assert.Contains(result, e => e.FirstName == "Jane" && e.LastName == "Smith");
    }

    [Fact]
    public async Task Handle_IncludeInactiveFalse_FiltersInactiveEmployees()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        var department = new Department
        {
            Id = departmentId,
            Name = "Engineering",
            IsActive = true,
            Employees = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Active",
                    LastName = "User",
                    Email = "active@example.com",
                    IsActive = true,
                    DepartmentId = departmentId
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Inactive",
                    LastName = "User",
                    Email = "inactive@example.com",
                    IsActive = false,
                    DepartmentId = departmentId
                }
            }
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(department);

        var query = new GetDepartmentEmployeesQuery { DepartmentId = departmentId, IncludeInactive = false };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Active", result[0].FirstName);
    }

    [Fact]
    public async Task Handle_IncludeInactiveTrue_ReturnsAllEmployees()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        var department = new Department
        {
            Id = departmentId,
            Name = "Engineering",
            IsActive = true,
            Employees = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Active",
                    LastName = "User",
                    Email = "active@example.com",
                    IsActive = true,
                    DepartmentId = departmentId
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Inactive",
                    LastName = "User",
                    Email = "inactive@example.com",
                    IsActive = false,
                    DepartmentId = departmentId
                }
            }
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(department);

        var query = new GetDepartmentEmployeesQuery { DepartmentId = departmentId, IncludeInactive = true };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
}
