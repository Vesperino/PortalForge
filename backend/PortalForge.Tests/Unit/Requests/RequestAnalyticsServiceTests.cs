using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Requests;

public class RequestAnalyticsServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRequestRepository> _mockRequestRepo;
    private readonly Mock<IRequestAnalyticsRepository> _mockAnalyticsRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<ILogger<RequestAnalyticsService>> _mockLogger;
    private readonly RequestAnalyticsService _service;

    public RequestAnalyticsServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRequestRepo = new Mock<IRequestRepository>();
        _mockAnalyticsRepo = new Mock<IRequestAnalyticsRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<RequestAnalyticsService>>();

        _mockUnitOfWork.Setup(u => u.RequestRepository).Returns(_mockRequestRepo.Object);
        _mockUnitOfWork.Setup(u => u.RequestAnalyticsRepository).Returns(_mockAnalyticsRepo.Object);
        _mockUnitOfWork.Setup(u => u.UserRepository).Returns(_mockUserRepo.Object);

        _service = new RequestAnalyticsService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CalculateUserAnalyticsAsync_ValidUser_CalculatesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2024;
        var month = 11;
        var requests = CreateTestRequestsForAnalytics(userId, year, month);

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);
        _mockAnalyticsRepo.Setup(r => r.GetByUserAndPeriodAsync(userId, year, month))
            .ReturnsAsync((RequestAnalytics?)null); // No existing analytics

        // Act
        var result = await _service.CalculateUserAnalyticsAsync(userId, year, month);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Year.Should().Be(year);
        result.Month.Should().Be(month);
        result.TotalRequests.Should().Be(4); // 4 requests in November 2024
        result.ApprovedRequests.Should().Be(2);
        result.RejectedRequests.Should().Be(1);
        result.PendingRequests.Should().Be(1);
        result.AverageProcessingTime.Should().BeGreaterThan(0);

        _mockAnalyticsRepo.Verify(r => r.CreateAsync(It.IsAny<RequestAnalytics>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CalculateUserAnalyticsAsync_ExistingAnalytics_UpdatesExisting()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2024;
        var month = 11;
        var requests = CreateTestRequestsForAnalytics(userId, year, month);

        var existingAnalytics = new RequestAnalytics
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Year = year,
            Month = month,
            TotalRequests = 0, // Old values
            ApprovedRequests = 0,
            RejectedRequests = 0,
            PendingRequests = 0,
            AverageProcessingTime = 0,
            LastCalculated = DateTime.UtcNow.AddDays(-1)
        };

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);
        _mockAnalyticsRepo.Setup(r => r.GetByUserAndPeriodAsync(userId, year, month))
            .ReturnsAsync(existingAnalytics);

        // Act
        var result = await _service.CalculateUserAnalyticsAsync(userId, year, month);

        // Assert
        result.Should().Be(existingAnalytics);
        result.TotalRequests.Should().Be(4); // Updated values
        result.ApprovedRequests.Should().Be(2);
        result.RejectedRequests.Should().Be(1);
        result.PendingRequests.Should().Be(1);

        _mockAnalyticsRepo.Verify(r => r.UpdateAsync(existingAnalytics), Times.Once);
        _mockAnalyticsRepo.Verify(r => r.CreateAsync(It.IsAny<RequestAnalytics>()), Times.Never);
    }

    [Fact]
    public async Task GetPersonalAnalyticsAsync_ValidUser_ReturnsCompleteAnalytics()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2024;
        var requests = CreateYearlyTestRequests(userId, year);

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetPersonalAnalyticsAsync(userId, year);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Year.Should().Be(year);
        result.TotalRequests.Should().Be(12); // 12 requests across the year
        result.ApprovedRequests.Should().Be(8);
        result.RejectedRequests.Should().Be(2);
        result.PendingRequests.Should().Be(2);
        result.ApprovalRate.Should().BeApproximately(66.67, 0.1); // 8/12 * 100
        result.MonthlyBreakdown.Should().HaveCount(12);
        result.RequestTypeBreakdown.Should().NotBeEmpty();
        result.ProcessingTimeBreakdown.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProcessingTimeAnalyticsAsync_ValidData_CalculatesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2024;
        var requests = CreateRequestsWithProcessingTimes(userId, year);

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetProcessingTimeAnalyticsAsync(userId, year);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Year.Should().Be(year);
        result.AverageProcessingTime.Should().BeGreaterThan(0);
        result.MedianProcessingTime.Should().BeGreaterThan(0);
        result.MinProcessingTime.Should().BeGreaterThan(0);
        result.MaxProcessingTime.Should().BeGreaterThan(result.MinProcessingTime);
        result.ProcessingTimeBreakdown.Should().NotBeEmpty();
        result.ProcessingTimeBreakdown.Sum(p => p.Percentage).Should().BeApproximately(100, 0.1);
    }

    [Fact]
    public async Task CalculateAllUsersAnalyticsAsync_MultipleUsers_CalculatesForAll()
    {
        // Arrange
        var year = 2024;
        var month = 11;
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), FirstName = "User1" },
            new User { Id = Guid.NewGuid(), FirstName = "User2" },
            new User { Id = Guid.NewGuid(), FirstName = "User3" }
        };

        _mockUserRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        foreach (var user in users)
        {
            _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(user.Id))
                .ReturnsAsync(new List<Request>());
            _mockAnalyticsRepo.Setup(r => r.GetByUserAndPeriodAsync(user.Id, year, month))
                .ReturnsAsync((RequestAnalytics?)null);
        }

        // Act
        await _service.CalculateAllUsersAnalyticsAsync(year, month);

        // Assert
        _mockAnalyticsRepo.Verify(r => r.CreateAsync(It.IsAny<RequestAnalytics>()), Times.Exactly(3));
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Exactly(3));
    }

    [Fact]
    public async Task RecalculateUserAnalyticsAsync_CurrentMonth_RecalculatesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentDate = DateTime.UtcNow;

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(new List<Request>());
        _mockAnalyticsRepo.Setup(r => r.GetByUserAndPeriodAsync(userId, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((RequestAnalytics?)null);

        // Act
        await _service.RecalculateUserAnalyticsAsync(userId);

        // Assert
        _mockAnalyticsRepo.Verify(r => r.CreateAsync(It.IsAny<RequestAnalytics>()), Times.AtLeastOnce);
    }

    private List<Request> CreateTestRequestsForAnalytics(Guid userId, int year, int month)
    {
        var baseDate = new DateTime(year, month, 1);
        var template = new RequestTemplate { Id = Guid.NewGuid(), Name = "Test Template" };

        return new List<Request>
        {
            new Request
            {
                Id = Guid.NewGuid(),
                SubmittedById = userId,
                SubmittedAt = baseDate.AddDays(1),
                Status = RequestStatus.Approved,
                CompletedAt = baseDate.AddDays(3),
                RequestTemplate = template
            },
            new Request
            {
                Id = Guid.NewGuid(),
                SubmittedById = userId,
                SubmittedAt = baseDate.AddDays(5),
                Status = RequestStatus.Approved,
                CompletedAt = baseDate.AddDays(7),
                RequestTemplate = template
            },
            new Request
            {
                Id = Guid.NewGuid(),
                SubmittedById = userId,
                SubmittedAt = baseDate.AddDays(10),
                Status = RequestStatus.Rejected,
                CompletedAt = baseDate.AddDays(12),
                RequestTemplate = template
            },
            new Request
            {
                Id = Guid.NewGuid(),
                SubmittedById = userId,
                SubmittedAt = baseDate.AddDays(15),
                Status = RequestStatus.InReview,
                RequestTemplate = template
            },
            // Request from different month (should be excluded)
            new Request
            {
                Id = Guid.NewGuid(),
                SubmittedById = userId,
                SubmittedAt = baseDate.AddMonths(1),
                Status = RequestStatus.Approved,
                RequestTemplate = template
            }
        };
    }

    private List<Request> CreateYearlyTestRequests(Guid userId, int year)
    {
        var requests = new List<Request>();
        var template = new RequestTemplate { Id = Guid.NewGuid(), Name = "Test Template" };

        for (int month = 1; month <= 12; month++)
        {
            var baseDate = new DateTime(year, month, 1);
            requests.Add(new Request
            {
                Id = Guid.NewGuid(),
                SubmittedById = userId,
                SubmittedAt = baseDate,
                Status = month <= 8 ? RequestStatus.Approved : 
                        month <= 10 ? RequestStatus.Rejected : RequestStatus.InReview,
                CompletedAt = month <= 10 ? baseDate.AddDays(2) : null,
                RequestTemplate = template
            });
        }

        return requests;
    }

    private List<Request> CreateRequestsWithProcessingTimes(Guid userId, int year)
    {
        var baseDate = new DateTime(year, 1, 1);
        var template = new RequestTemplate { Id = Guid.NewGuid(), Name = "Test Template" };

        return new List<Request>
        {
            // 12 hours processing time
            new Request
            {
                Id = Guid.NewGuid(),
                SubmittedById = userId,
                SubmittedAt = baseDate,
                Status = RequestStatus.Approved,
                CompletedAt = baseDate.AddHours(12),
                RequestTemplate = template
            },
            // 48 hours processing time
            new Request
            {
                Id = Guid.NewGuid(),
                SubmittedById = userId,
                SubmittedAt = baseDate.AddDays(10),
                Status = RequestStatus.Approved,
                CompletedAt = baseDate.AddDays(10).AddHours(48),
                RequestTemplate = template
            },
            // 120 hours processing time
            new Request
            {
                Id = Guid.NewGuid(),
                SubmittedById = userId,
                SubmittedAt = baseDate.AddDays(20),
                Status = RequestStatus.Approved,
                CompletedAt = baseDate.AddDays(20).AddHours(120),
                RequestTemplate = template
            },
            // No completion date (should be excluded from processing time calculations)
            new Request
            {
                Id = Guid.NewGuid(),
                SubmittedById = userId,
                SubmittedAt = baseDate.AddDays(30),
                Status = RequestStatus.InReview,
                CompletedAt = null,
                RequestTemplate = template
            }
        };
    }
}