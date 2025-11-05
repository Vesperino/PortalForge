using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Services;

namespace PortalForge.Tests.Unit.Infrastructure.Services;

public class EnhancedVacationCalculationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<EnhancedVacationCalculationService>> _loggerMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IVacationScheduleRepository> _vacationScheduleRepositoryMock;
    private readonly EnhancedVacationCalculationService _service;

    public EnhancedVacationCalculationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<EnhancedVacationCalculationService>>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _vacationScheduleRepositoryMock = new Mock<IVacationScheduleRepository>();

        _unitOfWorkMock.Setup(x => x.UserRepository).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.VacationScheduleRepository).Returns(_vacationScheduleRepositoryMock.Object);

        _service = new EnhancedVacationCalculationService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    #region ValidateCircumstantialLeaveAsync Tests

    [Fact]
    public async Task ValidateCircumstantialLeaveAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.ValidateCircumstantialLeaveAsync(
            userId, DateTime.Today, DateTime.Today.AddDays(1), "wedding", true);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Użytkownik nie istnieje");
    }

    [Fact]
    public async Task ValidateCircumstantialLeaveAsync_InvalidDateRange_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act - end date before start date
        var result = await _service.ValidateCircumstantialLeaveAsync(
            userId, DateTime.Today.AddDays(5), DateTime.Today, "wedding", true);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Nieprawidłowy zakres dat urlopu");
    }

    [Fact]
    public async Task ValidateCircumstantialLeaveAsync_WeddingWithDocumentation_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var startDate = new DateTime(2025, 1, 6); // Monday
        var endDate = new DateTime(2025, 1, 7);   // Tuesday (2 business days)

        // Act
        var result = await _service.ValidateCircumstantialLeaveAsync(
            userId, startDate, endDate, "wedding", true);

        // Assert
        result.IsValid.Should().BeTrue();
        result.DaysRequested.Should().Be(2);
        result.ReasonCategory.Should().Be("wedding");
        result.MaxAllowedDays.Should().Be(2);
        result.DocumentationRequired.Should().BeTrue();
        result.DocumentationSufficient.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCircumstantialLeaveAsync_WeddingWithoutDocumentation_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var startDate = new DateTime(2025, 1, 6); // Monday
        var endDate = new DateTime(2025, 1, 7);   // Tuesday

        // Act
        var result = await _service.ValidateCircumstantialLeaveAsync(
            userId, startDate, endDate, "wedding", false);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("wymaga załączenia dokumentacji");
    }

    [Fact]
    public async Task ValidateCircumstantialLeaveAsync_ExceedsMaxDays_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var startDate = new DateTime(2025, 1, 6); // Monday
        var endDate = new DateTime(2025, 1, 8);   // Wednesday (3 business days)

        // Act
        var result = await _service.ValidateCircumstantialLeaveAsync(
            userId, startDate, endDate, "wedding", true);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("może trwać maksymalnie 2 dni");
    }

    [Fact]
    public async Task ValidateCircumstantialLeaveAsync_PolishReasonMapping_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var startDate = new DateTime(2025, 1, 6); // Monday
        var endDate = new DateTime(2025, 1, 7);   // Tuesday

        // Act - using Polish reason
        var result = await _service.ValidateCircumstantialLeaveAsync(
            userId, startDate, endDate, "ślub", true);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ReasonCategory.Should().Be("wedding");
    }

    [Fact]
    public async Task ValidateCircumstantialLeaveAsync_MovingWithoutDocumentation_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var startDate = new DateTime(2025, 1, 6); // Monday
        var endDate = new DateTime(2025, 1, 6);   // Same day (1 business day)

        // Act - moving doesn't require documentation
        var result = await _service.ValidateCircumstantialLeaveAsync(
            userId, startDate, endDate, "moving", false);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ReasonCategory.Should().Be("moving");
        result.DocumentationRequired.Should().BeFalse();
    }

    #endregion

    #region ValidateOnDemandVacationAsync Tests

    [Fact]
    public async Task ValidateOnDemandVacationAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.ValidateOnDemandVacationAsync(
            userId, DateTime.Today, DateTime.Today.AddDays(1));

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Użytkownik nie istnieje");
    }

    [Fact]
    public async Task ValidateOnDemandVacationAsync_InvalidDateRange_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act - end date before start date
        var result = await _service.ValidateOnDemandVacationAsync(
            userId, DateTime.Today.AddDays(5), DateTime.Today);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Nieprawidłowy zakres dat urlopu");
    }

    [Fact]
    public async Task ValidateOnDemandVacationAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.OnDemandVacationDaysUsed = 2; // 2 used, 2 remaining
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var startDate = new DateTime(2025, 1, 6); // Monday
        var endDate = new DateTime(2025, 1, 7);   // Tuesday (2 business days)

        // Act
        var result = await _service.ValidateOnDemandVacationAsync(userId, startDate, endDate);

        // Assert
        result.IsValid.Should().BeTrue();
        result.DaysRequested.Should().Be(2);
        result.DaysUsedThisYear.Should().Be(2);
        result.DaysRemaining.Should().Be(0); // 4 - 2 - 2 = 0
        result.MaxAllowedDaysPerYear.Should().Be(4);
        result.Year.Should().Be(DateTime.UtcNow.Year);
    }

    [Fact]
    public async Task ValidateOnDemandVacationAsync_ExceedsRemainingDays_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.OnDemandVacationDaysUsed = 3; // 3 used, 1 remaining
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var startDate = new DateTime(2025, 1, 6); // Monday
        var endDate = new DateTime(2025, 1, 7);   // Tuesday (2 business days)

        // Act
        var result = await _service.ValidateOnDemandVacationAsync(userId, startDate, endDate);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Brak wystarczającej liczby dni urlopu na żądanie");
        result.ErrorMessage.Should().Contain("Dostępne: 1 dni, żądano: 2 dni");
    }

    [Fact]
    public async Task ValidateOnDemandVacationAsync_AllDaysUsed_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.OnDemandVacationDaysUsed = 4; // All 4 days used
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var startDate = new DateTime(2025, 1, 6); // Monday
        var endDate = new DateTime(2025, 1, 6);   // Same day (1 business day)

        // Act
        var result = await _service.ValidateOnDemandVacationAsync(userId, startDate, endDate);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Brak wystarczającej liczby dni urlopu na żądanie");
    }

    #endregion

    #region CheckVacationConflictsAsync Tests

    [Fact]
    public async Task CheckVacationConflictsAsync_UserNotFound_ReturnsConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.CheckVacationConflictsAsync(
            userId, DateTime.Today, DateTime.Today.AddDays(5));

        // Assert
        result.HasConflicts.Should().BeTrue();
        result.Conflicts.Should().HaveCount(1);
        result.Conflicts[0].Type.Should().Be(VacationConflictType.OverlappingVacation);
        result.Conflicts[0].Severity.Should().Be(ConflictSeverity.Critical);
    }

    [Fact]
    public async Task CheckVacationConflictsAsync_NoConflicts_ReturnsNoConflicts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.DepartmentId = departmentId;

        user.SupervisorId = null; // No supervisor to avoid supervisor conflicts
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _vacationScheduleRepositoryMock.Setup(x => x.GetByUserAsync(userId))
            .ReturnsAsync(new List<VacationSchedule>());
        _vacationScheduleRepositoryMock.Setup(x => x.GetTeamVacationsAsync(departmentId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<VacationSchedule>());
        
        // Create a larger team to ensure adequate coverage
        var teamMembers = new List<User>();
        for (int i = 0; i < 10; i++)
        {
            var member = CreateTestUser(Guid.NewGuid());
            member.DepartmentId = departmentId;
            teamMembers.Add(member);
        }
        teamMembers.Add(user); // Add the requesting user
        
        _userRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(teamMembers);

        var startDate = new DateTime(2025, 1, 6);
        var endDate = new DateTime(2025, 1, 10);

        // Act
        var result = await _service.CheckVacationConflictsAsync(userId, startDate, endDate);

        // Assert
        result.HasConflicts.Should().BeFalse();
        result.CanBeApproved.Should().BeTrue();
        result.CoverageAnalysis.Should().NotBeNull();
        result.CoverageAnalysis.IsAdequateCoverage.Should().BeTrue();
    }

    [Fact]
    public async Task CheckVacationConflictsAsync_OverlappingVacation_ReturnsConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);

        var existingVacation = new VacationSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartDate = new DateTime(2025, 1, 8),
            EndDate = new DateTime(2025, 1, 12),
            Status = VacationStatus.Scheduled
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _vacationScheduleRepositoryMock.Setup(x => x.GetByUserAsync(userId))
            .ReturnsAsync(new List<VacationSchedule> { existingVacation });

        var startDate = new DateTime(2025, 1, 6);
        var endDate = new DateTime(2025, 1, 10); // Overlaps with existing vacation

        // Act
        var result = await _service.CheckVacationConflictsAsync(userId, startDate, endDate);

        // Assert
        result.HasConflicts.Should().BeTrue();
        result.Conflicts.Should().HaveCount(1);
        result.Conflicts[0].Type.Should().Be(VacationConflictType.OverlappingVacation);
        result.Conflicts[0].Severity.Should().Be(ConflictSeverity.Critical);
        result.CanBeApproved.Should().BeFalse();
    }

    [Fact]
    public async Task CheckVacationConflictsAsync_InsufficientTeamCoverage_ReturnsConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.DepartmentId = departmentId;

        // Create a small team where one person on vacation creates insufficient coverage
        var otherUserId = Guid.NewGuid();
        var otherUser = CreateTestUser(otherUserId);
        otherUser.DepartmentId = departmentId;
        
        var teamMembers = new List<User>
        {
            user,
            otherUser // Only 2 people in team
        };

        var existingVacation = new VacationSchedule
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            StartDate = new DateTime(2025, 1, 6),
            EndDate = new DateTime(2025, 1, 10),
            Status = VacationStatus.Scheduled
        };

        user.SupervisorId = null; // No supervisor to avoid supervisor conflicts

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _vacationScheduleRepositoryMock.Setup(x => x.GetByUserAsync(userId))
            .ReturnsAsync(new List<VacationSchedule>());
        _vacationScheduleRepositoryMock.Setup(x => x.GetTeamVacationsAsync(departmentId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<VacationSchedule> { existingVacation });
        _userRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(teamMembers);

        var startDate = new DateTime(2025, 1, 6);
        var endDate = new DateTime(2025, 1, 10);

        // Act
        var result = await _service.CheckVacationConflictsAsync(userId, startDate, endDate);

        // Assert
        result.HasConflicts.Should().BeTrue();
        result.Conflicts.Should().Contain(c => c.Type == VacationConflictType.InsufficientCoverage);
        result.CoverageAnalysis.IsAdequateCoverage.Should().BeFalse();
        result.CoverageAnalysis.CoveragePercentage.Should().Be(0); // 0% coverage (both on vacation)
    }

    #endregion

    #region GetRemainingOnDemandDaysAsync Tests

    [Fact]
    public async Task GetRemainingOnDemandDaysAsync_UserNotFound_ReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetRemainingOnDemandDaysAsync(userId);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetRemainingOnDemandDaysAsync_NoUsedDays_ReturnsFour()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.OnDemandVacationDaysUsed = 0;
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _service.GetRemainingOnDemandDaysAsync(userId);

        // Assert
        result.Should().Be(4);
    }

    [Fact]
    public async Task GetRemainingOnDemandDaysAsync_TwoUsedDays_ReturnsTwo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.OnDemandVacationDaysUsed = 2;
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _service.GetRemainingOnDemandDaysAsync(userId);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetRemainingOnDemandDaysAsync_AllUsedDays_ReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.OnDemandVacationDaysUsed = 4;
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _service.GetRemainingOnDemandDaysAsync(userId);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetRemainingOnDemandDaysAsync_ExceedsLimit_ReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.OnDemandVacationDaysUsed = 6; // More than allowed (edge case)
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _service.GetRemainingOnDemandDaysAsync(userId);

        // Assert
        result.Should().Be(0, "should not return negative values");
    }

    [Fact]
    public async Task GetRemainingOnDemandDaysAsync_SpecificYear_UsesCurrentYear()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.OnDemandVacationDaysUsed = 1;
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _service.GetRemainingOnDemandDaysAsync(userId, 2025);

        // Assert
        result.Should().Be(3, "should calculate based on current user data regardless of year parameter");
    }

    #endregion

    private User CreateTestUser(Guid userId)
    {
        return new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Jan",
            LastName = "Kowalski",
            AnnualVacationDays = 26,
            VacationDaysUsed = 0,
            CarriedOverVacationDays = 0,
            OnDemandVacationDaysUsed = 0,
            CircumstantialLeaveDaysUsed = 0,
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}