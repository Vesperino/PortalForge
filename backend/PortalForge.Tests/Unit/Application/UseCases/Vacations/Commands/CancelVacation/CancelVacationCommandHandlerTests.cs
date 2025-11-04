using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Application.UseCases.Vacations.Commands.CancelVacation;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Application.UseCases.Vacations.Commands.CancelVacation;

public class CancelVacationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<IUnifiedValidatorService> _validatorServiceMock;
    private readonly Mock<ILogger<CancelVacationCommandHandler>> _loggerMock;
    private readonly CancelVacationCommandHandler _handler;

    public CancelVacationCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationServiceMock = new Mock<INotificationService>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _validatorServiceMock = new Mock<IUnifiedValidatorService>();
        _loggerMock = new Mock<ILogger<CancelVacationCommandHandler>>();

        _handler = new CancelVacationCommandHandler(
            _unitOfWorkMock.Object,
            _notificationServiceMock.Object,
            _auditLogServiceMock.Object,
            _validatorServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_VacationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var command = new CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = Guid.NewGuid(),
            Reason = "Test"
        };

        _validatorServiceMock.Setup(x => x.ValidateAsync(command))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.VacationScheduleRepository.GetByIdAsync(vacationId))
            .ReturnsAsync((VacationSchedule?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Vacation schedule with ID {vacationId} not found");
    }

    [Fact]
    public async Task Handle_AdminCancelsVacation_Success()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var substituteId = Guid.NewGuid();

        var vacation = CreateVacationSchedule(vacationId, employeeId, substituteId);
        var admin = CreateUser(adminId, UserRole.Admin);
        var employee = CreateUser(employeeId, UserRole.Employee);
        employee.VacationDaysUsed = 10;

        var command = new CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = adminId,
            Reason = "Zmiana planów firmowych"
        };

        SetupMocks(vacation, admin, employee);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        vacation.Status.Should().Be(VacationStatus.Cancelled);
        employee.VacationDaysUsed.Should().Be(5, "10 - 5 (vacation days) = 5");

        _unitOfWorkMock.Verify(x => x.VacationScheduleRepository.UpdateAsync(vacation), Times.Once);
        _unitOfWorkMock.Verify(x => x.UserRepository.UpdateAsync(employee), Times.Once);
    }

    [Fact]
    public async Task Handle_ApproverCancelsWithinOneDayOfStart_Success()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var vacation = CreateVacationSchedule(vacationId, employeeId, Guid.Empty);
        vacation.StartDate = DateTime.UtcNow.AddHours(-12); // Started 12 hours ago (within 1 day)
        vacation.SourceRequest = new Request
        {
            Id = Guid.NewGuid(),
            ApprovalSteps = new List<RequestApprovalStep>
            {
                new RequestApprovalStep
                {
                    ApproverId = approverId,
                    Status = ApprovalStepStatus.Approved
                }
            }
        };

        var approver = CreateUser(approverId, UserRole.Manager);
        var employee = CreateUser(employeeId, UserRole.Employee);
        employee.VacationDaysUsed = 8;

        var command = new CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = approverId,
            Reason = "Pilna sytuacja w firmie"
        };

        SetupMocks(vacation, approver, employee);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        vacation.Status.Should().Be(VacationStatus.Cancelled);
        // Note: DaysCount is computed. With StartDate = Now-12h and EndDate = Now+5days, Days = 6
        employee.VacationDaysUsed.Should().Be(2, "8 - 6 = 2 days");
    }

    [Fact]
    public async Task Handle_NotApproverNotAdmin_ThrowsForbiddenException()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var randomUserId = Guid.NewGuid();

        var vacation = CreateVacationSchedule(vacationId, employeeId, Guid.Empty);
        vacation.SourceRequest = new Request
        {
            Id = Guid.NewGuid(),
            ApprovalSteps = new List<RequestApprovalStep>()
        };

        var randomUser = CreateUser(randomUserId, UserRole.Employee);
        var employee = CreateUser(employeeId, UserRole.Employee);

        var command = new CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = randomUserId,
            Reason = "Test"
        };

        SetupMocks(vacation, randomUser, employee);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Nie masz uprawnień do anulowania tego urlopu");
    }

    [Fact]
    public async Task Handle_ApproverCancelsAfterOneDay_ThrowsValidationException()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var vacation = CreateVacationSchedule(vacationId, employeeId, Guid.Empty);
        vacation.StartDate = DateTime.UtcNow.AddDays(-2); // Started 2 days ago
        vacation.SourceRequest = new Request
        {
            Id = Guid.NewGuid(),
            ApprovalSteps = new List<RequestApprovalStep>
            {
                new RequestApprovalStep
                {
                    ApproverId = approverId,
                    Status = ApprovalStepStatus.Approved
                }
            }
        };

        var approver = CreateUser(approverId, UserRole.Manager);
        var employee = CreateUser(employeeId, UserRole.Employee);

        var command = new CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = approverId,
            Reason = "Too late"
        };

        SetupMocks(vacation, approver, employee);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*1 dnia*rozpoczęciu*");
    }

    [Fact]
    public async Task Handle_ValidCancellation_CreatesAuditLog()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var vacation = CreateVacationSchedule(vacationId, employeeId, Guid.Empty);
        vacation.Status = VacationStatus.Active;

        var admin = CreateUser(adminId, UserRole.Admin);
        var employee = CreateUser(employeeId, UserRole.Employee);

        var command = new CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = adminId,
            Reason = "Administrator decision"
        };

        SetupMocks(vacation, admin, employee);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _auditLogServiceMock.Verify(x => x.LogActionAsync(
            "VacationSchedule",
            vacationId.ToString(),
            "VacationCancelled",
            adminId,
            "Active",
            "Cancelled",
            "Administrator decision",
            null), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCancellation_NotifiesEmployee()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var vacation = CreateVacationSchedule(vacationId, employeeId, Guid.Empty);
        var admin = CreateUser(adminId, UserRole.Admin);
        var employee = CreateUser(employeeId, UserRole.Employee);

        var command = new CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = adminId,
            Reason = "Urgent company matter"
        };

        SetupMocks(vacation, admin, employee);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
            employeeId,
            NotificationType.VacationCancelled,
            "Urlop został anulowany",
            It.Is<string>(msg => msg.Contains("Urgent company matter") && msg.Contains("5 dni")),
            "VacationSchedule",
            vacationId.ToString(),
            "/dashboard/account"), Times.Once);
    }

    [Fact]
    public async Task Handle_VacationWithSubstitute_NotifiesSubstitute()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var substituteId = Guid.NewGuid();

        var vacation = CreateVacationSchedule(vacationId, employeeId, substituteId);
        var admin = CreateUser(adminId, UserRole.Admin);
        var employee = CreateUser(employeeId, UserRole.Employee);
        employee.FirstName = "Jan";
        employee.LastName = "Kowalski";

        var command = new CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = adminId,
            Reason = "Cancelled"
        };

        SetupMocks(vacation, admin, employee);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
            substituteId,
            NotificationType.System,
            "Urlop został anulowany",
            It.Is<string>(msg => msg.Contains("Jan Kowalski") && msg.Contains("zastępcą")),
            null,
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task Handle_VacationWithoutSubstitute_DoesNotNotifySubstitute()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var vacation = CreateVacationSchedule(vacationId, employeeId, Guid.Empty); // No substitute
        var admin = CreateUser(adminId, UserRole.Admin);
        var employee = CreateUser(employeeId, UserRole.Employee);

        var command = new CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = adminId,
            Reason = "Test"
        };

        SetupMocks(vacation, admin, employee);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - should notify employee only, not substitute
        _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
            employeeId,
            NotificationType.VacationCancelled,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);

        _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
            It.IsAny<Guid>(),
            It.IsAny<NotificationType>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once, "should only notify employee, not substitute");
    }

    [Fact]
    public async Task Handle_ValidCancellation_CallsValidatorService()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var vacation = CreateVacationSchedule(vacationId, employeeId, Guid.Empty);
        var admin = CreateUser(adminId, UserRole.Admin);
        var employee = CreateUser(employeeId, UserRole.Employee);

        var command = new CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = adminId,
            Reason = "Test"
        };

        SetupMocks(vacation, admin, employee);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _validatorServiceMock.Verify(x => x.ValidateAsync(command), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCancellation_LogsInformation()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var vacation = CreateVacationSchedule(vacationId, employeeId, Guid.Empty);
        var admin = CreateUser(adminId, UserRole.Admin);
        var employee = CreateUser(employeeId, UserRole.Employee);

        var command = new CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = adminId,
            Reason = "Logging test"
        };

        SetupMocks(vacation, admin, employee);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Vacation") && v.ToString()!.Contains("cancelled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleDaysVacation_ReturnsCorrectAmountToDaysPool()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var vacation = CreateVacationSchedule(vacationId, employeeId, Guid.Empty);
        vacation.StartDate = new DateTime(2025, 7, 1);
        vacation.EndDate = new DateTime(2025, 7, 10); // 10 days total

        var admin = CreateUser(adminId, UserRole.Admin);
        var employee = CreateUser(employeeId, UserRole.Employee);
        employee.VacationDaysUsed = 15;

        var command = new CancelVacationCommand
        {
            VacationScheduleId = vacationId,
            CancelledByUserId = adminId,
            Reason = "Test"
        };

        SetupMocks(vacation, admin, employee);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // DaysCount = (2025-07-10 - 2025-07-01).Days + 1 = 10 days
        employee.VacationDaysUsed.Should().Be(5, "15 - 10 (DaysCount from vacation) = 5");
    }

    // Helper methods
    private VacationSchedule CreateVacationSchedule(Guid id, Guid userId, Guid substituteId)
    {
        return new VacationSchedule
        {
            Id = id,
            UserId = userId,
            SubstituteUserId = substituteId,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(5),
            Status = VacationStatus.Scheduled,
            SourceRequestId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
    }

    private User CreateUser(Guid id, UserRole role)
    {
        return new User
        {
            Id = id,
            Email = $"{id}@test.com",
            FirstName = "Test",
            LastName = "User",
            Role = role,
            CreatedAt = DateTime.UtcNow,
            AnnualVacationDays = 26,
            VacationDaysUsed = 0,
            OnDemandVacationDaysUsed = 0,
            CircumstantialLeaveDaysUsed = 0,
            CarriedOverVacationDays = 0
        };
    }

    private void SetupMocks(VacationSchedule vacation, User cancelledBy, User employee)
    {
        _validatorServiceMock.Setup(x => x.ValidateAsync(It.IsAny<CancelVacationCommand>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.VacationScheduleRepository.GetByIdAsync(vacation.Id))
            .ReturnsAsync(vacation);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(cancelledBy.Id))
            .ReturnsAsync(cancelledBy);

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(employee.Id))
            .ReturnsAsync(employee);
    }
}
