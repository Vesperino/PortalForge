using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Application.UseCases.Requests.Commands.BulkApproveRequests;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using Xunit;

namespace PortalForge.Tests.Unit.Application.UseCases.Requests.Commands.BulkApproveRequests;

public class BulkApproveRequestsCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IEnhancedRequestRoutingService> _routingServiceMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<ILogger<BulkApproveRequestsCommandHandler>> _loggerMock;
    private readonly BulkApproveRequestsCommandHandler _handler;

    public BulkApproveRequestsCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationServiceMock = new Mock<INotificationService>();
        _routingServiceMock = new Mock<IEnhancedRequestRoutingService>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _loggerMock = new Mock<ILogger<BulkApproveRequestsCommandHandler>>();

        _handler = new BulkApproveRequestsCommandHandler(
            _unitOfWorkMock.Object,
            _notificationServiceMock.Object,
            _routingServiceMock.Object,
            _auditLogServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidSteps_ProcessesAllSuccessfully()
    {
        // Arrange
        var approverId = Guid.NewGuid();
        var approver = new User { Id = approverId, FullName = "Approver", IsActive = true };
        
        var stepId1 = Guid.NewGuid();
        var stepId2 = Guid.NewGuid();
        
        var request1 = new Request 
        { 
            Id = Guid.NewGuid(), 
            RequestTemplate = new RequestTemplate { Name = "Request 1" },
            SubmittedById = Guid.NewGuid(),
            Status = RequestStatus.InReview,
            ApprovalSteps = new List<RequestApprovalStep>()
        };
        
        var request2 = new Request 
        { 
            Id = Guid.NewGuid(), 
            RequestTemplate = new RequestTemplate { Name = "Request 2" },
            SubmittedById = Guid.NewGuid(),
            Status = RequestStatus.InReview,
            ApprovalSteps = new List<RequestApprovalStep>()
        };

        var step1 = new RequestApprovalStep
        {
            Id = stepId1,
            ApproverId = approverId,
            Status = ApprovalStepStatus.InReview,
            RequiresQuiz = false,
            Request = request1,
            StepOrder = 1
        };

        var step2 = new RequestApprovalStep
        {
            Id = stepId2,
            ApproverId = approverId,
            Status = ApprovalStepStatus.InReview,
            RequiresQuiz = false,
            Request = request2,
            StepOrder = 1
        };

        request1.ApprovalSteps.Add(step1);
        request2.ApprovalSteps.Add(step2);

        var command = new BulkApproveRequestsCommand
        {
            RequestStepIds = new List<Guid> { stepId1, stepId2 },
            ApproverId = approverId,
            Comment = "Bulk approval test"
        };

        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(approverId))
            .ReturnsAsync(approver);
        _unitOfWorkMock.Setup(u => u.RequestApprovalStepRepository.GetByIdAsync(stepId1))
            .ReturnsAsync(step1);
        _unitOfWorkMock.Setup(u => u.RequestApprovalStepRepository.GetByIdAsync(stepId2))
            .ReturnsAsync(step2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.SuccessfulApprovals);
        Assert.Equal(0, result.FailedApprovals);
        Assert.Empty(result.Errors);

        // Verify steps were updated
        Assert.Equal(ApprovalStepStatus.Approved, step1.Status);
        Assert.Equal(ApprovalStepStatus.Approved, step2.Status);
        Assert.NotNull(step1.FinishedAt);
        Assert.NotNull(step2.FinishedAt);
        Assert.Contains("Bulk approved", step1.Comment);
        Assert.Contains("Bulk approved", step2.Comment);

        // Verify repository calls
        _unitOfWorkMock.Verify(u => u.RequestApprovalStepRepository.UpdateAsync(step1), Times.Once);
        _unitOfWorkMock.Verify(u => u.RequestApprovalStepRepository.UpdateAsync(step2), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);

        // Verify audit log
        _auditLogServiceMock.Verify(a => a.LogActionAsync(
            "Request",
            It.IsAny<string>(),
            It.IsAny<string>(),
            approverId,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentApprover_ReturnsFailure()
    {
        // Arrange
        var approverId = Guid.NewGuid();
        var command = new BulkApproveRequestsCommand
        {
            RequestStepIds = new List<Guid> { Guid.NewGuid() },
            ApproverId = approverId
        };

        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(approverId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Approver not found", result.Message);
        Assert.Equal(0, result.SuccessfulApprovals);
        Assert.Equal(0, result.FailedApprovals);
    }
}