using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.Departments.Queries.GetDepartmentById;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Departments;

public class GetDepartmentByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<GetDepartmentByIdQueryHandler>> _mockLogger;
    private readonly GetDepartmentByIdQueryHandler _handler;

    public GetDepartmentByIdQueryHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<GetDepartmentByIdQueryHandler>>();
        _handler = new GetDepartmentByIdQueryHandler(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_DepartmentExists_ReturnsDepartmentDto()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var headId = Guid.NewGuid();

        var department = new Department
        {
            Id = departmentId,
            Name = "Engineering",
            Description = "Engineering Department",
            ParentDepartmentId = parentId,
            HeadOfDepartmentId = headId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ParentDepartment = new Department
            {
                Id = parentId,
                Name = "Technology"
            },
            HeadOfDepartment = new User
            {
                Id = headId,
                FirstName = "John",
                LastName = "Doe"
            },
            Employees = new List<User>
            {
                new User { Id = Guid.NewGuid() },
                new User { Id = Guid.NewGuid() },
                new User { Id = Guid.NewGuid() }
            }
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(department);

        var query = new GetDepartmentByIdQuery { DepartmentId = departmentId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(departmentId, result.Id);
        Assert.Equal("Engineering", result.Name);
        Assert.Equal("Engineering Department", result.Description);
        Assert.Equal(parentId, result.ParentDepartmentId);
        Assert.Equal("Technology", result.ParentDepartmentName);
        Assert.Equal(headId, result.DepartmentHeadId);
        Assert.Equal("John Doe", result.DepartmentHeadName);
        Assert.True(result.IsActive);
        Assert.Equal(3, result.EmployeeCount);
    }

    [Fact]
    public async Task Handle_DepartmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        var query = new GetDepartmentByIdQuery { DepartmentId = departmentId };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DepartmentWithoutParent_ReturnsNullParentFields()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        var department = new Department
        {
            Id = departmentId,
            Name = "Root Department",
            Description = "Top level",
            ParentDepartmentId = null,
            ParentDepartment = null,
            HeadOfDepartmentId = null,
            HeadOfDepartment = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Employees = new List<User>()
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(department);

        var query = new GetDepartmentByIdQuery { DepartmentId = departmentId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ParentDepartmentId);
        Assert.Null(result.ParentDepartmentName);
        Assert.Null(result.DepartmentHeadId);
        Assert.Null(result.DepartmentHeadName);
        Assert.Equal(0, result.EmployeeCount);
    }

    [Fact]
    public async Task Handle_DepartmentWithoutHead_ReturnsNullHeadFields()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        var department = new Department
        {
            Id = departmentId,
            Name = "New Department",
            HeadOfDepartmentId = null,
            HeadOfDepartment = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Employees = new List<User>()
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(department);

        var query = new GetDepartmentByIdQuery { DepartmentId = departmentId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.DepartmentHeadId);
        Assert.Null(result.DepartmentHeadName);
    }
}
