using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Common.Settings;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.Users.Queries.GetUserVacationSummary;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Application.UseCases.Users.Queries.GetUserVacationSummary;

public class GetUserVacationSummaryQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<PortalForge.Application.Interfaces.IVacationCalculationService> _vacationServiceMock;
    private readonly Mock<IOptions<VacationSettings>> _vacationSettingsMock;
    private readonly Mock<ILogger<GetUserVacationSummaryQueryHandler>> _loggerMock;
    private readonly GetUserVacationSummaryQueryHandler _handler;

    public GetUserVacationSummaryQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vacationServiceMock = new Mock<PortalForge.Application.Interfaces.IVacationCalculationService>();
        _vacationSettingsMock = new Mock<IOptions<VacationSettings>>();
        _vacationSettingsMock.Setup(x => x.Value).Returns(new VacationSettings
        {
            DefaultAnnualDays = 26,
            MaxOnDemandDays = 4,
            MaxCircumstantialDaysPerEvent = 2
        });
        _loggerMock = new Mock<ILogger<GetUserVacationSummaryQueryHandler>>();
        _handler = new GetUserVacationSummaryQueryHandler(_unitOfWorkMock.Object, _vacationServiceMock.Object, _vacationSettingsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserVacationSummaryQuery { UserId = userId };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID {userId} not found");
    }

    [Fact]
    public async Task Handle_ValidUser_ReturnsVacationSummaryDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Jan",
            LastName = "Kowalski",
            AnnualVacationDays = 26,
            VacationDaysUsed = 10,
            OnDemandVacationDaysUsed = 2,
            CircumstantialLeaveDaysUsed = 0,
            CarriedOverVacationDays = 5,
            CarriedOverExpiryDate = new DateTime(2025, 9, 30),
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };

        var query = new GetUserVacationSummaryQuery { UserId = userId };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        _vacationServiceMock.Setup(x => x.CalculateVacationDaysUsedAsync(userId, It.IsAny<int>())).ReturnsAsync(user.VacationDaysUsed ?? 0);
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AnnualVacationDays.Should().Be(26);
        result.VacationDaysUsed.Should().Be(10);
        result.VacationDaysRemaining.Should().Be(16, "26 - 10 = 16");
        result.OnDemandVacationDaysUsed.Should().Be(2);
        result.OnDemandVacationDaysRemaining.Should().Be(2, "4 - 2 = 2");
        result.CircumstantialLeaveDaysUsed.Should().Be(0);
        result.CarriedOverVacationDays.Should().Be(5);
        result.CarriedOverExpiryDate.Should().Be(new DateTime(2025, 9, 30));
        result.TotalAvailableVacationDays.Should().Be(21, "26 - 10 + 5 = 21");
    }

    [Fact]
    public async Task Handle_UserWithNoCarriedOverDays_ReturnsCorrectSummary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Anna",
            LastName = "Nowak",
            AnnualVacationDays = 20,
            VacationDaysUsed = 5,
            OnDemandVacationDaysUsed = 0,
            CircumstantialLeaveDaysUsed = 2,
            CarriedOverVacationDays = 0,
            CarriedOverExpiryDate = null,
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };

        var query = new GetUserVacationSummaryQuery { UserId = userId };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _vacationServiceMock.Setup(x => x.CalculateVacationDaysUsedAsync(userId, It.IsAny<int>())).ReturnsAsync(user.VacationDaysUsed ?? 0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.VacationDaysRemaining.Should().Be(15, "20 - 5 = 15");
        result.OnDemandVacationDaysRemaining.Should().Be(4, "4 - 0 = 4");
        result.CarriedOverVacationDays.Should().Be(0);
        result.CarriedOverExpiryDate.Should().BeNull();
        result.TotalAvailableVacationDays.Should().Be(15, "20 - 5 + 0 = 15");
    }

    [Fact]
    public async Task Handle_UserWithAllOnDemandDaysUsed_ReturnsZeroRemaining()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Piotr",
            LastName = "Zieliński",
            AnnualVacationDays = 26,
            VacationDaysUsed = 20,
            OnDemandVacationDaysUsed = 4, // All 4 used
            CircumstantialLeaveDaysUsed = 0,
            CarriedOverVacationDays = 0,
            CarriedOverExpiryDate = null,
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };

        var query = new GetUserVacationSummaryQuery { UserId = userId };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _vacationServiceMock.Setup(x => x.CalculateVacationDaysUsedAsync(userId, It.IsAny<int>())).ReturnsAsync(user.VacationDaysUsed ?? 0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.OnDemandVacationDaysUsed.Should().Be(4);
        result.OnDemandVacationDaysRemaining.Should().Be(0, "4 - 4 = 0, all on-demand days used");
    }

    [Fact]
    public async Task Handle_LogsInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            AnnualVacationDays = 26,
            VacationDaysUsed = 0,
            OnDemandVacationDaysUsed = 0,
            CircumstantialLeaveDaysUsed = 0,
            CarriedOverVacationDays = 0,
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };

        var query = new GetUserVacationSummaryQuery { UserId = userId };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _vacationServiceMock.Setup(x => x.CalculateVacationDaysUsedAsync(userId, It.IsAny<int>())).ReturnsAsync(user.VacationDaysUsed ?? 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving vacation summary for user")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserWithMaxCarriedOverDays_CalculatesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Maria",
            LastName = "Kowalska",
            AnnualVacationDays = 26,
            VacationDaysUsed = 0,
            OnDemandVacationDaysUsed = 0,
            CircumstantialLeaveDaysUsed = 0,
            CarriedOverVacationDays = 26, // Max carried over
            CarriedOverExpiryDate = new DateTime(2025, 9, 30),
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow
        };

        var query = new GetUserVacationSummaryQuery { UserId = userId };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalAvailableVacationDays.Should().Be(52, "26 annual + 26 carried over = 52");
        result.CarriedOverVacationDays.Should().Be(26);
        result.VacationDaysRemaining.Should().Be(26);
    }
}
