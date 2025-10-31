using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Departments.Queries.GetDepartmentTree;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Departments;

public class GetDepartmentTreeQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<GetDepartmentTreeQueryHandler>> _mockLogger;
    private readonly GetDepartmentTreeQueryHandler _handler;

    public GetDepartmentTreeQueryHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<GetDepartmentTreeQueryHandler>>();
        _handler = new GetDepartmentTreeQueryHandler(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_NoActiveDepartments_ReturnsEmptyList()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetAllAsync())
            .ReturnsAsync(new List<Department>());

        var query = new GetDepartmentTreeQuery { IncludeInactive = false };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_OnlyActiveDepartments_ReturnsTree()
    {
        // Arrange
        var rootDept = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Engineering",
            IsActive = true
        };

        var childDept = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Backend Team",
            ParentDepartmentId = rootDept.Id,
            IsActive = true
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetAllAsync())
            .ReturnsAsync(new List<Department> { rootDept, childDept });

        var query = new GetDepartmentTreeQuery { IncludeInactive = false };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Only root department
        Assert.Equal("Engineering", result[0].Name);
        Assert.Single(result[0].Children); // One child
        Assert.Equal("Backend Team", result[0].Children[0].Name);
    }

    [Fact]
    public async Task Handle_IncludeInactiveTrue_ReturnsAllDepartments()
    {
        // Arrange
        var activeDept = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Active Dept",
            IsActive = true
        };

        var inactiveDept = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Dept",
            IsActive = false
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetAllAsync())
            .ReturnsAsync(new List<Department> { activeDept, inactiveDept });

        var query = new GetDepartmentTreeQuery { IncludeInactive = true };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Both departments returned
    }

    [Fact]
    public async Task Handle_IncludeInactiveFalse_FiltersInactiveDepartments()
    {
        // Arrange
        var activeDept = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Active Dept",
            IsActive = true
        };

        var inactiveDept = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Dept",
            IsActive = false
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetAllAsync())
            .ReturnsAsync(new List<Department> { activeDept, inactiveDept });

        var query = new GetDepartmentTreeQuery { IncludeInactive = false };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Only active department
        Assert.Equal("Active Dept", result[0].Name);
    }

    [Fact]
    public async Task Handle_NestedHierarchy_BuildsCorrectTree()
    {
        // Arrange
        var rootId = Guid.NewGuid();
        var child1Id = Guid.NewGuid();
        var child2Id = Guid.NewGuid();

        var root = new Department
        {
            Id = rootId,
            Name = "Company",
            IsActive = true
        };

        var child1 = new Department
        {
            Id = child1Id,
            Name = "Engineering",
            ParentDepartmentId = rootId,
            IsActive = true
        };

        var child2 = new Department
        {
            Id = child2Id,
            Name = "Backend",
            ParentDepartmentId = child1Id,
            IsActive = true
        };

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetAllAsync())
            .ReturnsAsync(new List<Department> { root, child1, child2 });

        var query = new GetDepartmentTreeQuery { IncludeInactive = false };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // One root
        Assert.Equal("Company", result[0].Name);
        Assert.Single(result[0].Children); // One child under root
        Assert.Equal("Engineering", result[0].Children[0].Name);
        Assert.Single(result[0].Children[0].Children); // One grandchild
        Assert.Equal("Backend", result[0].Children[0].Children[0].Name);
    }
}
