using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Application.Services;

public class VacationScheduleServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IVacationScheduleRepository> _mockVacationRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<VacationScheduleService>> _mockLogger;
    private readonly VacationScheduleService _service;

    public VacationScheduleServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockVacationRepo = new Mock<IVacationScheduleRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<VacationScheduleService>>();

        _mockUnitOfWork.Setup(u => u.VacationScheduleRepository).Returns(_mockVacationRepo.Object);
        _mockUnitOfWork.Setup(u => u.UserRepository).Returns(_mockUserRepo.Object);

        _service = new VacationScheduleService(
            _mockUnitOfWork.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);
    }

    #region CreateFromApprovedRequestAsync Tests

    [Fact]
    public async Task CreateFromApprovedRequestAsync_ValidRequest_CreatesVacationSchedule()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var substituteId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        var substitute = new User
        {
            Id = substituteId,
            FirstName = "John",
            LastName = "Substitute",
            Email = "john.sub@test.com",
            IsActive = true
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(substituteId))
            .ReturnsAsync(substitute);

        var formData = new Dictionary<string, object>
        {
            ["startDate"] = "2025-11-01",
            ["endDate"] = "2025-11-10",
            ["substitute"] = substituteId.ToString()
        };

        var request = new Request
        {
            Id = requestId,
            SubmittedById = userId,
            FormData = JsonSerializer.Serialize(formData),
            Status = RequestStatus.Approved
        };

        // Act
        await _service.CreateFromApprovedRequestAsync(request);

        // Assert
        _mockVacationRepo.Verify(r => r.CreateAsync(
            It.Is<VacationSchedule>(s =>
                s.UserId == userId &&
                s.SubstituteUserId == substituteId &&
                s.StartDate == new DateTime(2025, 11, 1) &&
                s.EndDate == new DateTime(2025, 11, 10) &&
                s.Status == VacationStatus.Scheduled
            )),
            Times.Once);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockNotificationService.Verify(
            s => s.NotifySubstituteAsync(substituteId, It.IsAny<VacationSchedule>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateFromApprovedRequestAsync_SubstituteIsSelf_ThrowsValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var formData = new Dictionary<string, object>
        {
            ["startDate"] = "2025-11-01",
            ["endDate"] = "2025-11-10",
            ["substitute"] = userId.ToString() // Same as submitter!
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            SubmittedById = userId,
            FormData = JsonSerializer.Serialize(formData)
        };

        // Act
        Func<Task> act = async () => await _service.CreateFromApprovedRequestAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*własnym zastępcą*");
    }

    [Fact]
    public async Task CreateFromApprovedRequestAsync_SubstituteNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentSubstituteId = Guid.NewGuid();

        _mockUserRepo.Setup(r => r.GetByIdAsync(nonExistentSubstituteId))
            .ReturnsAsync((User?)null);

        var formData = new Dictionary<string, object>
        {
            ["startDate"] = "2025-11-01",
            ["endDate"] = "2025-11-10",
            ["substitute"] = nonExistentSubstituteId.ToString()
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            SubmittedById = userId,
            FormData = JsonSerializer.Serialize(formData)
        };

        // Act
        Func<Task> act = async () => await _service.CreateFromApprovedRequestAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetActiveSubstituteAsync Tests

    [Fact]
    public async Task GetActiveSubstituteAsync_UserOnVacation_ReturnsSubstitute()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var substituteId = Guid.NewGuid();

        var substitute = new User
        {
            Id = substituteId,
            FirstName = "John",
            LastName = "Substitute",
            Email = "john@test.com"
        };

        var vacation = new VacationSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SubstituteUserId = substituteId,
            Substitute = substitute,
            StartDate = DateTime.UtcNow.Date.AddDays(-1),
            EndDate = DateTime.UtcNow.Date.AddDays(5),
            Status = VacationStatus.Active,
            SourceRequestId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        _mockVacationRepo.Setup(r => r.GetActiveVacationAsync(userId))
            .ReturnsAsync(vacation);

        // Act
        var result = await _service.GetActiveSubstituteAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(substituteId);
    }

    [Fact]
    public async Task GetActiveSubstituteAsync_UserNotOnVacation_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockVacationRepo.Setup(r => r.GetActiveVacationAsync(userId))
            .ReturnsAsync((VacationSchedule?)null);

        // Act
        var result = await _service.GetActiveSubstituteAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetTeamCalendarAsync Tests

    [Fact]
    public async Task GetTeamCalendarAsync_NoVacations_ReturnsEmptyCalendar()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var startDate = new DateTime(2025, 11, 1);
        var endDate = new DateTime(2025, 11, 30);

        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "User1",
                LastName = "Test",
                Email = "user1@test.com",
                DepartmentId = departmentId,
                IsActive = true
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "User2",
                LastName = "Test",
                Email = "user2@test.com",
                DepartmentId = departmentId,
                IsActive = true
            }
        };

        _mockVacationRepo.Setup(r => r.GetTeamVacationsAsync(departmentId, startDate, endDate))
            .ReturnsAsync(new List<VacationSchedule>());

        _mockUserRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _service.GetTeamCalendarAsync(departmentId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Vacations.Should().BeEmpty();
        result.TeamSize.Should().Be(2);
        result.Alerts.Should().BeEmpty();
        result.Statistics.CurrentlyOnVacation.Should().Be(0);
    }

    #endregion

    #region GetMySubstitutionsAsync Tests

    [Fact]
    public async Task GetMySubstitutionsAsync_UserHasSubstitutions_ReturnsVacations()
    {
        // Arrange
        var substituteId = Guid.NewGuid();

        var vacations = new List<VacationSchedule>
        {
            new VacationSchedule
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SubstituteUserId = substituteId,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(5),
                Status = VacationStatus.Active,
                SourceRequestId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockVacationRepo.Setup(r => r.GetSubstitutionsAsync(substituteId))
            .ReturnsAsync(vacations);

        // Act
        var result = await _service.GetMySubstitutionsAsync(substituteId);

        // Assert
        result.Should().HaveCount(1);
        result.Should().AllSatisfy(v => v.SubstituteUserId.Should().Be(substituteId));
    }

    #endregion

    #region UpdateVacationStatusesAsync Tests

    [Fact]
    public async Task UpdateVacationStatusesAsync_ScheduledToActive_UpdatesStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var substituteId = Guid.NewGuid();

        var vacation = new VacationSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SubstituteUserId = substituteId,
            StartDate = DateTime.UtcNow.Date.AddDays(-1),
            EndDate = DateTime.UtcNow.Date.AddDays(5),
            Status = VacationStatus.Scheduled,
            SourceRequestId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        _mockVacationRepo.Setup(r => r.GetScheduledToActivateAsync())
            .ReturnsAsync(new List<VacationSchedule> { vacation });

        _mockVacationRepo.Setup(r => r.GetActiveToCompleteAsync())
            .ReturnsAsync(new List<VacationSchedule>());

        // Act
        await _service.UpdateVacationStatusesAsync();

        // Assert
        vacation.Status.Should().Be(VacationStatus.Active);

        _mockVacationRepo.Verify(r => r.UpdateAsync(vacation), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _mockNotificationService.Verify(
            s => s.NotifyVacationStartedAsync(substituteId, vacation),
            Times.Once);
    }

    #endregion
}
