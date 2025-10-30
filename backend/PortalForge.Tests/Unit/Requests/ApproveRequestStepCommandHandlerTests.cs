using Moq;
using Xunit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Requests.Commands.ApproveRequestStep;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Requests;

public class ApproveRequestStepCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRequestRepository> _mockRequestRepo;
    private readonly ApproveRequestStepCommandHandler _handler;

    public ApproveRequestStepCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRequestRepo = new Mock<IRequestRepository>();
        _mockUnitOfWork.Setup(u => u.RequestRepository).Returns(_mockRequestRepo.Object);
        _handler = new ApproveRequestStepCommandHandler(_mockUnitOfWork.Object);
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
        Assert.True(result.Success);
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
        Assert.True(result.Success);
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
        Assert.False(result.Success);
        Assert.Contains("Unauthorized", result.Message);
    }

    [Fact]
    public async Task Handle_QuizRequired_RequiresSurveyStatus()
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
                    QuizPassed = null
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

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Quiz must be completed", result.Message);
        Assert.Equal(ApprovalStepStatus.RequiresSurvey, request.ApprovalSteps.First().Status);
        Assert.Equal(RequestStatus.AwaitingSurvey, request.Status);
    }
}

