using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Users.Queries.SearchUsers;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Application.UseCases.Users.Queries.SearchUsers;

public class SearchUsersQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<SearchUsersQueryHandler>> _loggerMock;
    private readonly SearchUsersQueryHandler _handler;

    public SearchUsersQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<SearchUsersQueryHandler>>();
        _handler = new SearchUsersQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_QueryIsNull_ReturnsEmptyList()
    {
        // Arrange
        var query = new SearchUsersQuery { Query = null! };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _unitOfWorkMock.Verify(x => x.UserRepository.SearchAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<Guid?>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QueryIsEmpty_ReturnsEmptyList()
    {
        // Arrange
        var query = new SearchUsersQuery { Query = string.Empty };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_QueryIsTooShort_ReturnsEmptyList()
    {
        // Arrange
        var query = new SearchUsersQuery { Query = "a" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_QueryIsWhitespace_ReturnsEmptyList()
    {
        // Arrange
        var query = new SearchUsersQuery { Query = "   " };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ValidQuery_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var searchQuery = "John";
        var departmentId = Guid.NewGuid();
        var limit = 15;
        var onlyActive = true;

        var query = new SearchUsersQuery
        {
            Query = searchQuery,
            DepartmentId = departmentId,
            Limit = limit,
            OnlyActive = onlyActive
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.SearchAsync(
            searchQuery, onlyActive, departmentId, limit, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.UserRepository.SearchAsync(
            searchQuery, onlyActive, departmentId, limit, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsMappedUsers()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var users = new List<User>
        {
            new User
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Position = "Developer",
                Department = "Engineering",
                DepartmentId = departmentId,
                ProfilePhotoUrl = "https://example.com/photo.jpg"
            }
        };

        var query = new SearchUsersQuery { Query = "John" };

        _unitOfWorkMock.Setup(x => x.UserRepository.SearchAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<Guid?>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var userDto = result.First();
        userDto.Id.Should().Be(userId.ToString());
        userDto.FirstName.Should().Be("John");
        userDto.LastName.Should().Be("Doe");
        userDto.Email.Should().Be("john.doe@example.com");
        userDto.Position.Should().Be("Developer");
        userDto.Department.Should().Be("Engineering");
        userDto.DepartmentId.Should().Be(departmentId.ToString());
        userDto.ProfilePhotoUrl.Should().Be("https://example.com/photo.jpg");
    }

    [Fact]
    public async Task Handle_MultipleUsers_ReturnsAllMappedUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new User { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" },
            new User { Id = Guid.NewGuid(), FirstName = "Johnny", LastName = "Smith", Email = "johnny@example.com" }
        };

        var query = new SearchUsersQuery { Query = "Jo" };

        _unitOfWorkMock.Setup(x => x.UserRepository.SearchAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<Guid?>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(u => u.FirstName == "John");
        result.Should().Contain(u => u.FirstName == "Jane");
        result.Should().Contain(u => u.FirstName == "Johnny");
    }

    [Fact]
    public async Task Handle_NoUsersFound_ReturnsEmptyList()
    {
        // Arrange
        var query = new SearchUsersQuery { Query = "NonExistent" };

        _unitOfWorkMock.Setup(x => x.UserRepository.SearchAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<Guid?>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_DefaultParameters_UsesCorrectDefaults()
    {
        // Arrange
        var query = new SearchUsersQuery { Query = "Test" };

        _unitOfWorkMock.Setup(x => x.UserRepository.SearchAsync(
            "Test",
            true, // OnlyActive default
            null, // DepartmentId default
            10,   // Limit default
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.UserRepository.SearchAsync(
            "Test", true, null, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OnlyActiveIsFalse_PassesFalseToRepository()
    {
        // Arrange
        var query = new SearchUsersQuery { Query = "Test", OnlyActive = false };

        _unitOfWorkMock.Setup(x => x.UserRepository.SearchAsync(
            It.IsAny<string>(),
            false,
            It.IsAny<Guid?>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.UserRepository.SearchAsync(
            "Test", false, It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("abc")]
    [InlineData("John Doe")]
    public async Task Handle_QueryAtOrAboveMinLength_CallsRepository(string searchQuery)
    {
        // Arrange
        var query = new SearchUsersQuery { Query = searchQuery };

        _unitOfWorkMock.Setup(x => x.UserRepository.SearchAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<Guid?>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.UserRepository.SearchAsync(
            searchQuery,
            It.IsAny<bool>(),
            It.IsAny<Guid?>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserWithNullDepartmentId_MapsToNullDepartmentIdString()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                DepartmentId = null
            }
        };

        var query = new SearchUsersQuery { Query = "John" };

        _unitOfWorkMock.Setup(x => x.UserRepository.SearchAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<Guid?>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().DepartmentId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CancellationRequested_PassesCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var query = new SearchUsersQuery { Query = "Test" };

        _unitOfWorkMock.Setup(x => x.UserRepository.SearchAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<Guid?>(),
            It.IsAny<int>(),
            cts.Token))
            .ReturnsAsync(new List<User>());

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.UserRepository.SearchAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<Guid?>(),
            It.IsAny<int>(),
            cts.Token), Times.Once);
    }
}
