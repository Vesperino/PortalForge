using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.Services;
using PortalForge.Application.UseCases.Admin.Queries.GetAuditLogs;
using PortalForge.Domain.Entities;

namespace PortalForge.Tests.Unit.Application.UseCases.Admin.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUnifiedValidatorService> _validatorServiceMock;
    private readonly Mock<ILogger<GetAuditLogsQueryHandler>> _loggerMock;
    private readonly GetAuditLogsQueryHandler _handler;

    public GetAuditLogsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorServiceMock = new Mock<IUnifiedValidatorService>();
        _loggerMock = new Mock<ILogger<GetAuditLogsQueryHandler>>();

        _handler = new GetAuditLogsQueryHandler(
            _unitOfWorkMock.Object,
            _validatorServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ReturnsAllAuditLogs()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var auditLogs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "User",
                EntityId = userId1.ToString(),
                Action = "Created",
                UserId = userId1,
                User = new User { FirstName = "John", LastName = "Doe" },
                Timestamp = DateTime.UtcNow.AddDays(-1),
                OldValue = null,
                NewValue = "New user created"
            },
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "Request",
                EntityId = Guid.NewGuid().ToString(),
                Action = "Approved",
                UserId = userId2,
                User = new User { FirstName = "Jane", LastName = "Smith" },
                Timestamp = DateTime.UtcNow.AddHours(-2),
                OldValue = "Pending",
                NewValue = "Approved"
            }
        };

        var query = new GetAuditLogsQuery
        {
            Page = 1,
            PageSize = 50
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                null, null, null, null, null, 1, 50))
            .ReturnsAsync((auditLogs.Count, auditLogs));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(50);
        result.TotalPages.Should().Be(1);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();

        var firstLog = result.Items[0];
        firstLog.EntityType.Should().Be("User");
        firstLog.Action.Should().Be("Created");
        firstLog.UserFullName.Should().Be("John Doe");

        var secondLog = result.Items[1];
        secondLog.EntityType.Should().Be("Request");
        secondLog.Action.Should().Be("Approved");
        secondLog.UserFullName.Should().Be("Jane Smith");
        secondLog.OldValue.Should().Be("Pending");
        secondLog.NewValue.Should().Be("Approved");
    }

    [Fact]
    public async Task Handle_WithEntityTypeFilter_ReturnsFilteredLogs()
    {
        // Arrange
        var userLogs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "User",
                EntityId = Guid.NewGuid().ToString(),
                Action = "Updated",
                UserId = Guid.NewGuid(),
                User = new User { FirstName = "Admin", LastName = "User" },
                Timestamp = DateTime.UtcNow
            }
        };

        var query = new GetAuditLogsQuery
        {
            EntityType = "User",
            Page = 1,
            PageSize = 50
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                "User", null, null, null, null, 1, 50))
            .ReturnsAsync((userLogs.Count, userLogs));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.Should().OnlyContain(log => log.EntityType == "User");
    }

    [Fact]
    public async Task Handle_WithActionFilter_ReturnsFilteredLogs()
    {
        // Arrange
        var approvedLogs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "Request",
                EntityId = Guid.NewGuid().ToString(),
                Action = "Approved",
                UserId = Guid.NewGuid(),
                User = new User { FirstName = "Manager", LastName = "One" },
                Timestamp = DateTime.UtcNow
            }
        };

        var query = new GetAuditLogsQuery
        {
            Action = "Approved",
            Page = 1,
            PageSize = 50
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                null, "Approved", null, null, null, 1, 50))
            .ReturnsAsync((approvedLogs.Count, approvedLogs));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.Should().OnlyContain(log => log.Action == "Approved");
    }

    [Fact]
    public async Task Handle_WithUserIdFilter_ReturnsFilteredLogs()
    {
        // Arrange
        var specificUserId = Guid.NewGuid();
        var userLogs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "Request",
                EntityId = Guid.NewGuid().ToString(),
                Action = "Created",
                UserId = specificUserId,
                User = new User { FirstName = "Specific", LastName = "User" },
                Timestamp = DateTime.UtcNow
            }
        };

        var query = new GetAuditLogsQuery
        {
            UserId = specificUserId,
            Page = 1,
            PageSize = 50
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                null, null, specificUserId, null, null, 1, 50))
            .ReturnsAsync((userLogs.Count, userLogs));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.Should().OnlyContain(log => log.UserId == specificUserId);
    }

    [Fact]
    public async Task Handle_WithDateRangeFilter_ReturnsFilteredLogs()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow;

        var logsInRange = new List<AuditLog>
        {
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "User",
                EntityId = Guid.NewGuid().ToString(),
                Action = "Updated",
                UserId = Guid.NewGuid(),
                User = new User { FirstName = "John", LastName = "Doe" },
                Timestamp = DateTime.UtcNow.AddDays(-3)
            }
        };

        var query = new GetAuditLogsQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            Page = 1,
            PageSize = 50
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                null, null, null, fromDate, toDate, 1, 50))
            .ReturnsAsync((logsInRange.Count, logsInRange));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.Should().OnlyContain(log =>
            log.Timestamp >= fromDate && log.Timestamp <= toDate);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var allLogs = Enumerable.Range(1, 100).Select(i => new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = "User",
            EntityId = Guid.NewGuid().ToString(),
            Action = "Action" + i,
            UserId = Guid.NewGuid(),
            User = new User { FirstName = "User", LastName = i.ToString() },
            Timestamp = DateTime.UtcNow.AddMinutes(-i)
        }).ToList();

        var page2Logs = allLogs.Skip(20).Take(20).ToList();

        var query = new GetAuditLogsQuery
        {
            Page = 2,
            PageSize = 20
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                null, null, null, null, null, 2, 20))
            .ReturnsAsync((100, page2Logs));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(20);
        result.TotalCount.Should().Be(100);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(20);
        result.TotalPages.Should().Be(5);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithMultipleFilters_ReturnsFilteredLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fromDate = DateTime.UtcNow.AddDays(-30);
        var toDate = DateTime.UtcNow;

        var filteredLogs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "Request",
                EntityId = Guid.NewGuid().ToString(),
                Action = "Approved",
                UserId = userId,
                User = new User { FirstName = "John", LastName = "Doe" },
                Timestamp = DateTime.UtcNow.AddDays(-5),
                OldValue = "InReview",
                NewValue = "Approved",
                Reason = "All requirements met"
            }
        };

        var query = new GetAuditLogsQuery
        {
            EntityType = "Request",
            Action = "Approved",
            UserId = userId,
            FromDate = fromDate,
            ToDate = toDate,
            Page = 1,
            PageSize = 50
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                "Request", "Approved", userId, fromDate, toDate, 1, 50))
            .ReturnsAsync((filteredLogs.Count, filteredLogs));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        var log = result.Items[0];
        log.EntityType.Should().Be("Request");
        log.Action.Should().Be("Approved");
        log.UserId.Should().Be(userId);
        log.UserFullName.Should().Be("John Doe");
        log.OldValue.Should().Be("InReview");
        log.NewValue.Should().Be("Approved");
        log.Reason.Should().Be("All requirements met");
    }

    [Fact]
    public async Task Handle_WithNullUser_MapsUserFullNameAsNull()
    {
        // Arrange
        var systemLogs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "System",
                EntityId = "BackgroundJob",
                Action = "VacationAllowanceReset",
                UserId = null,
                User = null,
                Timestamp = DateTime.UtcNow,
                NewValue = "Annual reset completed"
            }
        };

        var query = new GetAuditLogsQuery
        {
            Page = 1,
            PageSize = 50
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                null, null, null, null, null, 1, 50))
            .ReturnsAsync((systemLogs.Count, systemLogs));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        var log = result.Items[0];
        log.UserId.Should().BeNull();
        log.UserFullName.Should().BeNull();
        log.Action.Should().Be("VacationAllowanceReset");
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyPagedResult()
    {
        // Arrange
        var query = new GetAuditLogsQuery
        {
            EntityType = "NonExistent",
            Page = 1,
            PageSize = 50
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                "NonExistent", null, null, null, null, 1, 50))
            .ReturnsAsync((0, new List<AuditLog>()));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CallsValidatorService()
    {
        // Arrange
        var query = new GetAuditLogsQuery
        {
            Page = 1,
            PageSize = 50
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(),
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((0, new List<AuditLog>()));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _validatorServiceMock.Verify(x => x.ValidateAsync(query), Times.Once);
    }

    [Fact]
    public async Task Handle_LogsInformation()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "User",
                EntityId = Guid.NewGuid().ToString(),
                Action = "Created",
                UserId = Guid.NewGuid(),
                User = new User { FirstName = "Test", LastName = "User" },
                Timestamp = DateTime.UtcNow
            }
        };

        var query = new GetAuditLogsQuery
        {
            Page = 1,
            PageSize = 50
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                null, null, null, null, null, 1, 50))
            .ReturnsAsync((logs.Count, logs));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieved") && v.ToString()!.Contains("audit logs")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithIpAddress_MapsCorrectly()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "User",
                EntityId = Guid.NewGuid().ToString(),
                Action = "Login",
                UserId = Guid.NewGuid(),
                User = new User { FirstName = "John", LastName = "Doe" },
                Timestamp = DateTime.UtcNow,
                IpAddress = "192.168.1.100"
            }
        };

        var query = new GetAuditLogsQuery
        {
            Page = 1,
            PageSize = 50
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                null, null, null, null, null, 1, 50))
            .ReturnsAsync((logs.Count, logs));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].IpAddress.Should().Be("192.168.1.100");
    }

    [Fact]
    public async Task Handle_LastPage_CalculatesPaginationCorrectly()
    {
        // Arrange
        var lastPageLogs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "User",
                EntityId = Guid.NewGuid().ToString(),
                Action = "Updated",
                UserId = Guid.NewGuid(),
                User = new User { FirstName = "Last", LastName = "User" },
                Timestamp = DateTime.UtcNow
            }
        };

        var query = new GetAuditLogsQuery
        {
            Page = 5,
            PageSize = 20
        };

        _validatorServiceMock
            .Setup(x => x.ValidateAsync(query))
            .Returns(Task.CompletedTask);

        // Total 81 items, page 5 with pageSize 20 = last page with 1 item
        _unitOfWorkMock.Setup(x => x.AuditLogRepository.GetFilteredAsync(
                null, null, null, null, null, 5, 20))
            .ReturnsAsync((81, lastPageLogs));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(81);
        result.Page.Should().Be(5);
        result.PageSize.Should().Be(20);
        result.TotalPages.Should().Be(5);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }
}
