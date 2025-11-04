using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Services;

namespace PortalForge.Tests.Unit.Infrastructure.Services;

public class VacationCalculationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<VacationCalculationService>> _loggerMock;
    private readonly VacationCalculationService _service;

    public VacationCalculationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<VacationCalculationService>>();
        _service = new VacationCalculationService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    #region CalculateProportionalVacationDays Tests

    [Fact]
    public void CalculateProportionalVacationDays_EmploymentInPreviousYear_ReturnsFullAnnualDays()
    {
        // Arrange
        var employmentStartDate = new DateTime(DateTime.UtcNow.Year - 1, 6, 15);
        var annualDays = 26;

        // Act
        var result = _service.CalculateProportionalVacationDays(employmentStartDate, annualDays);

        // Assert
        result.Should().Be(26, "employee starting in previous year is entitled to full annual allowance");
    }

    [Fact]
    public void CalculateProportionalVacationDays_EmploymentInJanuaryCurrentYear_ReturnsFullAnnualDays()
    {
        // Arrange
        var employmentStartDate = new DateTime(DateTime.UtcNow.Year, 1, 1);
        var annualDays = 26;

        // Act
        var result = _service.CalculateProportionalVacationDays(employmentStartDate, annualDays);

        // Assert
        result.Should().Be(26, "employee starting in January gets 12 months = full year");
    }

    [Fact]
    public void CalculateProportionalVacationDays_EmploymentInDecemberCurrentYear_ReturnsOnMonth()
    {
        // Arrange
        var employmentStartDate = new DateTime(DateTime.UtcNow.Year, 12, 15);
        var annualDays = 26;

        // Expected: (26 / 12) * 1 month = 2.17 → Math.Ceiling = 3 days
        var expected = 3;

        // Act
        var result = _service.CalculateProportionalVacationDays(employmentStartDate, annualDays);

        // Assert
        result.Should().Be(expected, "employee starting in December gets 1 month proportional vacation");
    }

    [Fact]
    public void CalculateProportionalVacationDays_EmploymentInJulyCurrentYear_ReturnsSixMonths()
    {
        // Arrange
        var employmentStartDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
        var annualDays = 26;

        // Expected: (26 / 12) * 6 months = 13 days
        var expected = 13;

        // Act
        var result = _service.CalculateProportionalVacationDays(employmentStartDate, annualDays);

        // Assert
        result.Should().Be(expected, "employee starting in July gets 6 months (Jul-Dec) proportional vacation");
    }

    [Fact]
    public void CalculateProportionalVacationDays_WithDifferentAnnualDays_CalculatesCorrectly()
    {
        // Arrange - employee with 10+ years service (20 days baseline + 6 bonus = 26)
        // But let's say they had 20 days before the bonus
        var employmentStartDate = new DateTime(DateTime.UtcNow.Year, 4, 1);
        var annualDays = 20;

        // Expected: (20 / 12) * 9 months (Apr-Dec) = 15 days
        var expected = 15;

        // Act
        var result = _service.CalculateProportionalVacationDays(employmentStartDate, annualDays);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateProportionalVacationDays_RoundsUpToFullDays()
    {
        // Arrange
        var employmentStartDate = new DateTime(DateTime.UtcNow.Year, 11, 1);
        var annualDays = 26;

        // Expected: (26 / 12) * 2 months = 4.33 → Math.Ceiling = 5 days
        var expected = 5;

        // Act
        var result = _service.CalculateProportionalVacationDays(employmentStartDate, annualDays);

        // Assert
        result.Should().Be(expected, "proportional days must always be rounded UP per Art. 155¹ KP");
    }

    #endregion

    #region CalculateBusinessDays Tests

    [Fact]
    public void CalculateBusinessDays_SameDay_Monday_ReturnsOne()
    {
        // Arrange - Monday, January 6, 2025
        var date = new DateTime(2025, 1, 6);

        // Act
        var result = _service.CalculateBusinessDays(date, date);

        // Assert
        result.Should().Be(1, "a single Monday should count as 1 business day");
    }

    [Fact]
    public void CalculateBusinessDays_SameDay_Saturday_ReturnsZero()
    {
        // Arrange - Saturday, January 4, 2025
        var date = new DateTime(2025, 1, 4);

        // Act
        var result = _service.CalculateBusinessDays(date, date);

        // Assert
        result.Should().Be(0, "Saturday is not a business day");
    }

    [Fact]
    public void CalculateBusinessDays_WeekendOnly_ReturnsZero()
    {
        // Arrange - Saturday to Sunday
        var startDate = new DateTime(2025, 1, 4);
        var endDate = new DateTime(2025, 1, 5);

        // Act
        var result = _service.CalculateBusinessDays(startDate, endDate);

        // Assert
        result.Should().Be(0, "weekend should have 0 business days");
    }

    [Fact]
    public void CalculateBusinessDays_FullWeek_ReturnsFive()
    {
        // Arrange - Monday to Sunday (Jan 6-12, 2025)
        var startDate = new DateTime(2025, 1, 6);
        var endDate = new DateTime(2025, 1, 12);

        // Act
        var result = _service.CalculateBusinessDays(startDate, endDate);

        // Assert
        result.Should().Be(5, "full week (Mon-Sun) has 5 business days");
    }

    [Fact]
    public void CalculateBusinessDays_MondayToFriday_ReturnsFive()
    {
        // Arrange - Monday to Friday (Jan 6-10, 2025)
        var startDate = new DateTime(2025, 1, 6);
        var endDate = new DateTime(2025, 1, 10);

        // Act
        var result = _service.CalculateBusinessDays(startDate, endDate);

        // Assert
        result.Should().Be(5, "Monday to Friday is 5 business days");
    }

    [Fact]
    public void CalculateBusinessDays_InvalidRange_ReturnsZero()
    {
        // Arrange - end date before start date
        var startDate = new DateTime(2025, 1, 10);
        var endDate = new DateTime(2025, 1, 5);

        // Act
        var result = _service.CalculateBusinessDays(startDate, endDate);

        // Assert
        result.Should().Be(0, "invalid date range should return 0");
    }

    [Fact]
    public void CalculateBusinessDays_TwoWeeks_ReturnsTen()
    {
        // Arrange - Two full weeks (Jan 6-19, 2025)
        var startDate = new DateTime(2025, 1, 6);
        var endDate = new DateTime(2025, 1, 19);

        // Act
        var result = _service.CalculateBusinessDays(startDate, endDate);

        // Assert
        result.Should().Be(10, "two full weeks have 10 business days");
    }

    #endregion

    #region CanTakeVacationAsync Tests

    [Fact]
    public async Task CanTakeVacationAsync_UserNotFound_ReturnsFalseWithError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.CanTakeVacationAsync(
            userId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(5),
            LeaveType.Annual);

        // Assert
        result.CanTake.Should().BeFalse();
        result.ErrorMessage.Should().Be("Użytkownik nie istnieje");
    }

    [Fact]
    public async Task CanTakeVacationAsync_InvalidDateRange_ReturnsFalseWithError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act - end date before start date
        var result = await _service.CanTakeVacationAsync(
            userId,
            new DateTime(2025, 1, 10),
            new DateTime(2025, 1, 5),
            LeaveType.Annual);

        // Assert
        result.CanTake.Should().BeFalse();
        result.ErrorMessage.Should().Be("Nieprawidłowy zakres dat urlopu");
    }

    [Fact]
    public async Task CanTakeVacationAsync_OnDemand_AllDaysUsed_ReturnsFalseWithError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.OnDemandVacationDaysUsed = 4; // All 4 days used

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.CanTakeVacationAsync(
            userId,
            new DateTime(2025, 1, 6), // Monday
            new DateTime(2025, 1, 6), // Same day
            LeaveType.OnDemand);

        // Assert
        result.CanTake.Should().BeFalse();
        result.ErrorMessage.Should().Be("Wykorzystano już wszystkie 4 dni urlopu na żądanie w tym roku");
    }

    [Fact]
    public async Task CanTakeVacationAsync_OnDemand_RequestingMoreThanRemaining_ReturnsFalseWithError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.OnDemandVacationDaysUsed = 2; // 2 used, 2 remaining

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act - requesting 3 days (Mon-Wed)
        var result = await _service.CanTakeVacationAsync(
            userId,
            new DateTime(2025, 1, 6), // Monday
            new DateTime(2025, 1, 8), // Wednesday
            LeaveType.OnDemand);

        // Assert
        result.CanTake.Should().BeFalse();
        result.ErrorMessage.Should().Be("Możesz wziąć jeszcze 2 dni urlopu na żądanie");
    }

    [Fact]
    public async Task CanTakeVacationAsync_OnDemand_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.OnDemandVacationDaysUsed = 2; // 2 used, 2 remaining

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act - requesting 2 days (Mon-Tue)
        var result = await _service.CanTakeVacationAsync(
            userId,
            new DateTime(2025, 1, 6), // Monday
            new DateTime(2025, 1, 7), // Tuesday
            LeaveType.OnDemand);

        // Assert
        result.CanTake.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task CanTakeVacationAsync_Circumstantial_MoreThanTwoDays_ReturnsFalseWithError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act - requesting 3 days (Mon-Wed)
        var result = await _service.CanTakeVacationAsync(
            userId,
            new DateTime(2025, 1, 6), // Monday
            new DateTime(2025, 1, 8), // Wednesday
            LeaveType.Circumstantial);

        // Assert
        result.CanTake.Should().BeFalse();
        result.ErrorMessage.Should().Be("Urlop okolicznościowy to maksymalnie 2 dni na wydarzenie");
    }

    [Fact]
    public async Task CanTakeVacationAsync_Circumstantial_TwoDays_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act - requesting 2 days (Mon-Tue)
        var result = await _service.CanTakeVacationAsync(
            userId,
            new DateTime(2025, 1, 6), // Monday
            new DateTime(2025, 1, 7), // Tuesday
            LeaveType.Circumstantial);

        // Assert
        result.CanTake.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task CanTakeVacationAsync_Annual_InsufficientDays_ReturnsFalseWithError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.AnnualVacationDays = 10;
        user.VacationDaysUsed = 5;
        user.CarriedOverVacationDays = 0;
        // TotalAvailableVacationDays = 10 - 5 + 0 = 5 days

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act - requesting 10 days (2 weeks)
        var result = await _service.CanTakeVacationAsync(
            userId,
            new DateTime(2025, 1, 6), // Monday
            new DateTime(2025, 1, 17), // Friday next week
            LeaveType.Annual);

        // Assert
        result.CanTake.Should().BeFalse();
        result.ErrorMessage.Should().Be("Brak wystarczającej liczby dni urlopu. Dostępne: 5 dni");
    }

    [Fact]
    public async Task CanTakeVacationAsync_Annual_SufficientDays_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.AnnualVacationDays = 26;
        user.VacationDaysUsed = 10;
        user.CarriedOverVacationDays = 5;
        // TotalAvailableVacationDays = 26 - 10 + 5 = 21 days

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act - requesting 10 days (2 weeks)
        var result = await _service.CanTakeVacationAsync(
            userId,
            new DateTime(2025, 1, 6), // Monday
            new DateTime(2025, 1, 17), // Friday next week
            LeaveType.Annual);

        // Assert
        result.CanTake.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task CanTakeVacationAsync_Sick_AlwaysAllowed_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.AnnualVacationDays = 0;
        user.VacationDaysUsed = 0;
        user.CarriedOverVacationDays = 0;
        // No vacation days available

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act - sick leave should always be allowed
        var result = await _service.CanTakeVacationAsync(
            userId,
            new DateTime(2025, 1, 6), // Monday
            new DateTime(2025, 1, 17), // Friday next week
            LeaveType.Sick);

        // Assert
        result.CanTake.Should().BeTrue("sick leave cannot be rejected per Polish law");
        result.ErrorMessage.Should().BeNull();
    }

    #endregion

    #region CalculateVacationDaysUsedAsync Tests

    [Fact]
    public async Task CalculateVacationDaysUsedAsync_NoVacations_ReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2025;

        _unitOfWorkMock.Setup(x => x.VacationScheduleRepository.GetAllAsync())
            .ReturnsAsync(new List<VacationSchedule>());

        // Act
        var result = await _service.CalculateVacationDaysUsedAsync(userId, year);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task CalculateVacationDaysUsedAsync_MultipleVacationsInYear_ReturnsSumOfDays()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2025;

        var vacations = new List<VacationSchedule>
        {
            new VacationSchedule
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartDate = new DateTime(2025, 1, 6),
                EndDate = new DateTime(2025, 1, 10), // 5 days
                Status = VacationStatus.Completed
            },
            new VacationSchedule
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartDate = new DateTime(2025, 3, 10),
                EndDate = new DateTime(2025, 3, 14), // 5 days
                Status = VacationStatus.Active
            }
        };

        _unitOfWorkMock.Setup(x => x.VacationScheduleRepository.GetAllAsync())
            .ReturnsAsync(vacations);

        // Act
        var result = await _service.CalculateVacationDaysUsedAsync(userId, year);

        // Assert
        result.Should().Be(10, "should sum 5 + 5 days from both vacations");
    }

    [Fact]
    public async Task CalculateVacationDaysUsedAsync_VacationSpanningYears_IncludesInBothYears()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2025;

        var vacations = new List<VacationSchedule>
        {
            new VacationSchedule
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartDate = new DateTime(2024, 12, 30), // Previous year
                EndDate = new DateTime(2025, 1, 3), // Current year (5 days)
                Status = VacationStatus.Completed
            }
        };

        _unitOfWorkMock.Setup(x => x.VacationScheduleRepository.GetAllAsync())
            .ReturnsAsync(vacations);

        // Act
        var result = await _service.CalculateVacationDaysUsedAsync(userId, year);

        // Assert
        result.Should().Be(5, "vacation spanning years should be included if any date is in target year");
    }

    [Fact]
    public async Task CalculateVacationDaysUsedAsync_OnlyCountsCompletedAndActive_ExcludesOtherStatuses()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2025;

        var vacations = new List<VacationSchedule>
        {
            new VacationSchedule
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartDate = new DateTime(2025, 1, 6),
                EndDate = new DateTime(2025, 1, 10), // 5 days
                Status = VacationStatus.Completed // Should count
            },
            new VacationSchedule
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartDate = new DateTime(2025, 2, 10),
                EndDate = new DateTime(2025, 2, 14), // 5 days
                Status = VacationStatus.Active // Should count
            },
            new VacationSchedule
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartDate = new DateTime(2025, 3, 10),
                EndDate = new DateTime(2025, 3, 14), // 5 days
                Status = VacationStatus.Cancelled // Should NOT count
            },
            new VacationSchedule
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartDate = new DateTime(2025, 4, 10),
                EndDate = new DateTime(2025, 4, 14), // 5 days
                Status = VacationStatus.Scheduled // Should NOT count
            }
        };

        _unitOfWorkMock.Setup(x => x.VacationScheduleRepository.GetAllAsync())
            .ReturnsAsync(vacations);

        // Act
        var result = await _service.CalculateVacationDaysUsedAsync(userId, year);

        // Assert
        result.Should().Be(10, "should only count Completed (5) + Active (5) statuses");
    }

    [Fact]
    public async Task CalculateVacationDaysUsedAsync_DifferentUser_ReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var year = 2025;

        var vacations = new List<VacationSchedule>
        {
            new VacationSchedule
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId, // Different user
                StartDate = new DateTime(2025, 1, 6),
                EndDate = new DateTime(2025, 1, 10), // 5 days
                Status = VacationStatus.Completed
            }
        };

        _unitOfWorkMock.Setup(x => x.VacationScheduleRepository.GetAllAsync())
            .ReturnsAsync(vacations);

        // Act
        var result = await _service.CalculateVacationDaysUsedAsync(userId, year);

        // Assert
        result.Should().Be(0, "should not count vacations for different user");
    }

    #endregion

    #region GetAvailableVacationDaysAsync Tests

    [Fact]
    public async Task GetAvailableVacationDaysAsync_UserNotFound_ReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetAvailableVacationDaysAsync(userId);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetAvailableVacationDaysAsync_ValidUser_ReturnsTotalAvailableDays()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.AnnualVacationDays = 26;
        user.VacationDaysUsed = 10;
        user.CarriedOverVacationDays = 5;
        // TotalAvailableVacationDays = 26 - 10 + 5 = 21

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetAvailableVacationDaysAsync(userId);

        // Assert
        result.Should().Be(21, "should return (AnnualVacationDays - VacationDaysUsed + CarriedOverVacationDays)");
    }

    [Fact]
    public async Task GetAvailableVacationDaysAsync_NoCarriedOverDays_ReturnsCorrectAmount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.AnnualVacationDays = 20;
        user.VacationDaysUsed = 5;
        user.CarriedOverVacationDays = 0;
        // TotalAvailableVacationDays = 20 - 5 + 0 = 15

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetAvailableVacationDaysAsync(userId);

        // Assert
        result.Should().Be(15);
    }

    [Fact]
    public async Task GetAvailableVacationDaysAsync_AllDaysUsed_ReturnsCarriedOverOnly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        user.AnnualVacationDays = 26;
        user.VacationDaysUsed = 26;
        user.CarriedOverVacationDays = 8;
        // TotalAvailableVacationDays = 26 - 26 + 8 = 8

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetAvailableVacationDaysAsync(userId);

        // Assert
        result.Should().Be(8, "should return only carried over days when all annual days are used");
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
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };
    }
}
