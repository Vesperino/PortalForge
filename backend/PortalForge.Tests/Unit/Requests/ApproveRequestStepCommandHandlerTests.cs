using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Application.UseCases.Requests.Commands.ApproveRequestStep;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Requests;

public class ApproveRequestStepCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRequestRepository> _mockRequestRepo;
    private readonly Mock<IRequestCommentRepository> _mockRequestCommentRepo;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IVacationScheduleService> _mockVacationService;
    private readonly Mock<IVacationDaysDeductionService> _mockVacationDaysDeductionService;
    private readonly Mock<ILogger<ApproveRequestStepCommandHandler>> _mockLogger;
    private readonly ApproveRequestStepCommandHandler _handler;

    public ApproveRequestStepCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRequestRepo = new Mock<IRequestRepository>();
        _mockRequestCommentRepo = new Mock<IRequestCommentRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockVacationService = new Mock<IVacationScheduleService>();
        _mockVacationDaysDeductionService = new Mock<IVacationDaysDeductionService>();
        _mockLogger = new Mock<ILogger<ApproveRequestStepCommandHandler>>();
        _mockUnitOfWork.Setup(u => u.RequestRepository).Returns(_mockRequestRepo.Object);
        _mockUnitOfWork.Setup(u => u.RequestCommentRepository).Returns(_mockRequestCommentRepo.Object);
        _handler = new ApproveRequestStepCommandHandler(
            _mockUnitOfWork.Object,
            _mockNotificationService.Object,
            _mockVacationService.Object,
            _mockVacationDaysDeductionService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidApproval_ApprovesStepSuccessfully()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var request = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-2025-0001",
            Status = RequestStatus.InReview,
            ApprovalSteps = new List<RequestApprovalStep>
            {
                new RequestApprovalStep
                {
                    Id = stepId,
                    StepOrder = 1,
                    ApproverId = approverId,
                    Status = ApprovalStepStatus.InReview,
                    RequiresQuiz = false
                }
            }
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        var command = new ApproveRequestStepCommand
        {
            RequestId = requestId,
            StepId = stepId,
            ApproverId = approverId,
            Comment = "Approved"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Request fully approved", result.Message);
        Assert.Equal(ApprovalStepStatus.Approved, request.ApprovalSteps.First().Status);
        Assert.Equal(RequestStatus.Approved, request.Status);
        Assert.NotNull(request.CompletedAt);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleSteps_MovesToNextStep()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var step1Id = Guid.NewGuid();
        var step2Id = Guid.NewGuid();
        var approver1Id = Guid.NewGuid();
        var approver2Id = Guid.NewGuid();

        var request = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-2025-0001",
            Status = RequestStatus.InReview,
            ApprovalSteps = new List<RequestApprovalStep>
            {
                new RequestApprovalStep
                {
                    Id = step1Id,
                    StepOrder = 1,
                    ApproverId = approver1Id,
                    Status = ApprovalStepStatus.InReview,
                    RequiresQuiz = false
                },
                new RequestApprovalStep
                {
                    Id = step2Id,
                    StepOrder = 2,
                    ApproverId = approver2Id,
                    Status = ApprovalStepStatus.Pending,
                    RequiresQuiz = false
                }
            }
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        var command = new ApproveRequestStepCommand
        {
            RequestId = requestId,
            StepId = step1Id,
            ApproverId = approver1Id
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Step approved, moved to next approver", result.Message);
        Assert.Equal(ApprovalStepStatus.Approved, request.ApprovalSteps.ElementAt(0).Status);
        Assert.Equal(ApprovalStepStatus.InReview, request.ApprovalSteps.ElementAt(1).Status);
        Assert.NotNull(request.ApprovalSteps.ElementAt(1).StartedAt);
        Assert.Equal(RequestStatus.InReview, request.Status);
    }

    [Fact]
    public async Task Handle_UnauthorizedApprover_ReturnsFalse()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var wrongApproverId = Guid.NewGuid();

        var request = new Request
        {
            Id = requestId,
            ApprovalSteps = new List<RequestApprovalStep>
            {
                new RequestApprovalStep
                {
                    Id = stepId,
                    ApproverId = approverId,
                    Status = ApprovalStepStatus.InReview
                }
            }
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        var command = new ApproveRequestStepCommand
        {
            RequestId = requestId,
            StepId = stepId,
            ApproverId = wrongApproverId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unauthorized", result.Message);
    }

    [Fact]
    public async Task Handle_QuizNotCompleted_StillAllowsApproval()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var request = new Request
        {
            Id = requestId,
            Status = RequestStatus.InReview,
            ApprovalSteps = new List<RequestApprovalStep>
            {
                new RequestApprovalStep
                {
                    Id = stepId,
                    ApproverId = approverId,
                    Status = ApprovalStepStatus.InReview,
                    RequiresQuiz = true,
                    QuizPassed = null // Quiz not completed yet
                }
            }
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        var command = new ApproveRequestStepCommand
        {
            RequestId = requestId,
            StepId = stepId,
            ApproverId = approverId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Quiz is informational only, approval should succeed
        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalStepStatus.Approved, request.ApprovalSteps.First().Status);
        Assert.Equal(RequestStatus.Approved, request.Status);
    }

    [Fact]
    public async Task Handle_QuizFailedButApproverApprovesAnyway_AllowsApproval()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var request = new Request
        {
            Id = requestId,
            Status = RequestStatus.InReview,
            ApprovalSteps = new List<RequestApprovalStep>
            {
                new RequestApprovalStep
                {
                    Id = stepId,
                    ApproverId = approverId,
                    Status = ApprovalStepStatus.InReview,
                    RequiresQuiz = true,
                    QuizScore = 40, // Failed quiz
                    QuizPassed = false
                }
            }
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        var command = new ApproveRequestStepCommand
        {
            RequestId = requestId,
            StepId = stepId,
            ApproverId = approverId,
            Comment = "Approved despite quiz failure - special circumstances"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Approver can approve even if quiz was failed
        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalStepStatus.Approved, request.ApprovalSteps.First().Status);
        Assert.Equal(RequestStatus.Approved, request.Status);
        Assert.Contains("special circumstances", request.ApprovalSteps.First().Comment ?? "");
    }

    [Fact]
    public async Task Handle_NextApproverOnVacation_RoutesToSubstitute()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var step1Id = Guid.NewGuid();
        var step2Id = Guid.NewGuid();
        var approver1Id = Guid.NewGuid();
        var approver2Id = Guid.NewGuid(); // Original approver (on vacation)
        var substituteId = Guid.NewGuid(); // Substitute

        var substitute = new User
        {
            Id = substituteId,
            FirstName = "John",
            LastName = "Substitute",
            Email = "john.sub@test.com"
        };

        var request = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-2025-0001",
            Status = RequestStatus.InReview,
            ApprovalSteps = new List<RequestApprovalStep>
            {
                new RequestApprovalStep
                {
                    Id = step1Id,
                    StepOrder = 1,
                    ApproverId = approver1Id,
                    Status = ApprovalStepStatus.InReview,
                    RequiresQuiz = false
                },
                new RequestApprovalStep
                {
                    Id = step2Id,
                    StepOrder = 2,
                    ApproverId = approver2Id,
                    Status = ApprovalStepStatus.Pending,
                    RequiresQuiz = false
                }
            }
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        // Setup: approver2 is on vacation, return substitute
        _mockVacationService.Setup(v => v.GetActiveSubstituteAsync(approver2Id))
            .ReturnsAsync(substitute);

        var command = new ApproveRequestStepCommand
        {
            RequestId = requestId,
            StepId = step1Id,
            ApproverId = approver1Id
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // Step 2 should now be assigned to substitute, not original approver
        var step2 = request.ApprovalSteps.ElementAt(1);
        Assert.Equal(substituteId, step2.ApproverId);
        Assert.Equal(ApprovalStepStatus.InReview, step2.Status);
        Assert.Contains("substitute", step2.Comment?.ToLower() ?? "");

        // Verify substitute was notified
        _mockNotificationService.Verify(
            n => n.NotifyApproverAsync(substituteId, request),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NextApproverNotOnVacation_RoutesNormally()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var step1Id = Guid.NewGuid();
        var step2Id = Guid.NewGuid();
        var approver1Id = Guid.NewGuid();
        var approver2Id = Guid.NewGuid();

        var request = new Request
        {
            Id = requestId,
            RequestNumber = "REQ-2025-0001",
            Status = RequestStatus.InReview,
            ApprovalSteps = new List<RequestApprovalStep>
            {
                new RequestApprovalStep
                {
                    Id = step1Id,
                    StepOrder = 1,
                    ApproverId = approver1Id,
                    Status = ApprovalStepStatus.InReview,
                    RequiresQuiz = false
                },
                new RequestApprovalStep
                {
                    Id = step2Id,
                    StepOrder = 2,
                    ApproverId = approver2Id,
                    Status = ApprovalStepStatus.Pending,
                    RequiresQuiz = false
                }
            }
        };

        _mockRequestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        // Setup: approver2 is NOT on vacation, return null
        _mockVacationService.Setup(v => v.GetActiveSubstituteAsync(approver2Id))
            .ReturnsAsync((User?)null);

        var command = new ApproveRequestStepCommand
        {
            RequestId = requestId,
            StepId = step1Id,
            ApproverId = approver1Id
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // Step 2 should still be assigned to original approver
        var step2 = request.ApprovalSteps.ElementAt(1);
        Assert.Equal(approver2Id, step2.ApproverId);
        Assert.Equal(ApprovalStepStatus.InReview, step2.Status);

        // Verify original approver was notified
        _mockNotificationService.Verify(
            n => n.NotifyApproverAsync(approver2Id, request),
            Times.Once);
    }
}
