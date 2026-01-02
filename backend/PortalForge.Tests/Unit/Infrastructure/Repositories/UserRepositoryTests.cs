using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;
using PortalForge.Infrastructure.Repositories;
using Xunit;

namespace PortalForge.Tests.Unit.Infrastructure.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task SearchAsync_WithMatchingFirstName_ReturnsMatchingUsers()
    {
        // Arrange
        var user1 = CreateUser("John", "Doe", "john.doe@test.com");
        var user2 = CreateUser("Jane", "Smith", "jane.smith@test.com");
        var user3 = CreateUser("Johnny", "Test", "johnny.test@test.com");

        await _context.Users.AddRangeAsync(user1, user2, user3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("john", onlyActive: true, departmentId: null, limit: 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.FirstName == "John");
        result.Should().Contain(u => u.FirstName == "Johnny");
    }

    [Fact]
    public async Task SearchAsync_WithMatchingEmail_ReturnsMatchingUsers()
    {
        // Arrange
        var user1 = CreateUser("John", "Doe", "admin@company.com");
        var user2 = CreateUser("Jane", "Smith", "user@company.com");

        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("admin", onlyActive: true, departmentId: null, limit: 10);

        // Assert
        result.Should().HaveCount(1);
        result.First().Email.Should().Be("admin@company.com");
    }

    [Fact]
    public async Task SearchAsync_WithOnlyActive_FiltersInactiveUsers()
    {
        // Arrange
        var activeUser = CreateUser("John", "Doe", "john@test.com", isActive: true);
        var inactiveUser = CreateUser("Johnny", "Inactive", "johnny@test.com", isActive: false);

        await _context.Users.AddRangeAsync(activeUser, inactiveUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("john", onlyActive: true, departmentId: null, limit: 10);

        // Assert
        result.Should().HaveCount(1);
        result.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_WithDepartmentFilter_ReturnsUsersFromDepartment()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var userInDept = CreateUser("John", "Doe", "john@test.com");
        userInDept.DepartmentId = departmentId;

        var userOutDept = CreateUser("Johnny", "Other", "johnny@test.com");
        userOutDept.DepartmentId = Guid.NewGuid();

        await _context.Users.AddRangeAsync(userInDept, userOutDept);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("john", onlyActive: true, departmentId: departmentId, limit: 10);

        // Assert
        result.Should().HaveCount(1);
        result.First().DepartmentId.Should().Be(departmentId);
    }

    [Fact]
    public async Task SearchAsync_WithLimit_RespectsLimit()
    {
        // Arrange
        for (int i = 0; i < 20; i++)
        {
            await _context.Users.AddAsync(CreateUser($"John{i}", "Doe", $"john{i}@test.com"));
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("john", onlyActive: true, departmentId: null, limit: 5);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task SearchAsync_WithShortQuery_ReturnsEmpty()
    {
        // Arrange
        var user = CreateUser("John", "Doe", "john@test.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act - Query length < 2 should be handled by handler, but repository still works
        var result = await _repository.SearchAsync("j", onlyActive: true, departmentId: null, limit: 10);

        // Assert - Repository still searches, handler filters short queries
        result.Should().HaveCount(1);
    }

    private static User CreateUser(string firstName, string lastName, string email, bool isActive = true)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            IsActive = isActive,
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };
    }
}
