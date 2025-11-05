using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using Xunit;

namespace PortalForge.Tests.Unit.Application.Services;

public class EnhancedRequestRoutingServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<EnhancedRequestRoutingService>> _loggerMock;
    private readonly EnhancedRequestRoutingService _service;

    public EnhancedRequestRoutingServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<EnhancedRequestRoutingService>>();
        _service = new EnhancedRequestRoutingService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ResolveParallelApproversAsync_WithUserGroup_ReturnsGroupMembers()
    {
        // Arrange
        var submitter = new User { Id = Guid.NewGuid(), FullName = "John Doe" };
        var groupId = Guid.NewGuid();
        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.UserGroup,
            ApproverGroupId = groupId,
            IsParallel = true,
            ParallelGroupId = "group1"
        };

        var groupUsers = new List<User>
        {
            new User { Id = Guid.NewGuid(), FullName = "Approver 1" },
            new User { Id = Guid.NewGuid(), FullName = "Approver 2" },
            submitter // Should be filtered out
        };

        _unitOfWorkMock.Setup(u => u.RoleGroupRepository.GetUsersInGroupAsync(groupId))
            .ReturnsAsync(groupUsers);

        // Act
        var result = await _service.ResolveParallelApproversAsync(stepTemplate, submitter);

        // Assert
        Assert.Equal(2, result.Count); // Submitter should be excluded
        Assert.DoesNotContain(result, u => u.Id == submitter.Id);
    }

    [Fact]
    public async Task ShouldEscalateAsync_WithTimeoutExceeded_ReturnsTrue()
    {
        // Arrange
        var escalationUserId = Guid.NewGuid();
        var stepTemplate = new RequestApprovalStepTemplate
        {
            EscalationTimeout = TimeSpan.FromHours(24),
            EscalationUserId = escalationUserId
        };

        var step = new RequestApprovalStep
        {
            Id = Guid.NewGuid(),
            StepTemplate = stepTemplate,
            CreatedAt = DateTime.UtcNow.AddHours(-25) // 25 hours ago
        };

        // Act
        var result = await _service.ShouldEscalateAsync(step);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShouldEscalateAsync_WithTimeoutNotExceeded_ReturnsFalse()
    {
        // Arrange
        var escalationUserId = Guid.NewGuid();
        var stepTemplate = new RequestApprovalStepTemplate
        {
            EscalationTimeout = TimeSpan.FromHours(24),
            EscalationUserId = escalationUserId
        };

        var step = new RequestApprovalStep
        {
            Id = Guid.NewGuid(),
            StepTemplate = stepTemplate,
            CreatedAt = DateTime.UtcNow.AddHours(-12) // 12 hours ago
        };

        // Act
        var result = await _service.ShouldEscalateAsync(step);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ShouldEscalateAsync_WithNoEscalationTimeout_ReturnsFalse()
    {
        // Arrange
        var stepTemplate = new RequestApprovalStepTemplate
        {
            EscalationTimeout = null,
            EscalationUserId = null
        };

        var step = new RequestApprovalStep
        {
            Id = Guid.NewGuid(),
            StepTemplate = stepTemplate,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = await _service.ShouldEscalateAsync(step);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EscalateRequestStepAsync_WithValidStep_UpdatesStepToEscalationUser()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var escalationUserId = Guid.NewGuid();
        var escalationUser = new User { Id = escalationUserId, FullName = "Escalation User" };
        
        var stepTemplate = new RequestApprovalStepTemplate
        {
            EscalationUserId = escalationUserId
        };

        var step = new RequestApprovalStep
        {
            Id = stepId,
            StepTemplate = stepTemplate,
            AssignedToUserId = Guid.NewGuid() // Different user initially
        };

        _unitOfWorkMock.Setup(u => u.RequestApprovalStepRepository.GetByIdAsync(stepId))
            .ReturnsAsync(step);
        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(escalationUserId))
            .ReturnsAsync(escalationUser);

        // Act
        var result = await _service.EscalateRequestStepAsync(stepId);

        // Assert
        Assert.Equal(escalationUserId, result.AssignedToUserId);
        Assert.Equal(escalationUser, result.AssignedToUser);
        Assert.NotNull(result.EscalatedAt);
        _unitOfWorkMock.Verify(u => u.RequestApprovalStepRepository.UpdateAsync(step), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetDelegatedApproversAsync_WithActiveDelegations_ReturnsDelegate()
    {
        // Arrange
        var originalApproverId = Guid.NewGuid();
        var delegateUserId = Guid.NewGuid();
        var delegateUser = new User { Id = delegateUserId, FullName = "Delegate User" };

        var activeDelegation = new ApprovalDelegation
        {
            FromUserId = originalApproverId,
            ToUserId = delegateUserId,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        _unitOfWorkMock.Setup(u => u.ApprovalDelegationRepository.GetAllAsync())
            .ReturnsAsync(new[] { activeDelegation });
        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(delegateUserId))
            .ReturnsAsync(delegateUser);

        // Act
        var result = await _service.GetDelegatedApproversAsync(originalApproverId);

        // Assert
        Assert.Single(result);
        Assert.Equal(delegateUserId, result.First().Id);
    }

    [Fact]
    public async Task SetApprovalDelegationAsync_WithValidUsers_CreatesDelegation()
    {
        // Arrange
        var fromUserId = Guid.NewGuid();
        var toUserId = Guid.NewGuid();
        var fromUser = new User { Id = fromUserId, FullName = "From User" };
        var toUser = new User { Id = toUserId, FullName = "To User" };
        var until = DateTime.UtcNow.AddDays(7);
        var reason = "Vacation";

        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(fromUserId))
            .ReturnsAsync(fromUser);
        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(toUserId))
            .ReturnsAsync(toUser);

        // Act
        var result = await _service.SetApprovalDelegationAsync(fromUserId, toUserId, until, reason);

        // Assert
        Assert.Equal(fromUserId, result.FromUserId);
        Assert.Equal(toUserId, result.ToUserId);
        Assert.Equal(until, result.EndDate);
        Assert.Equal(reason, result.Reason);
        Assert.True(result.IsActive);
        _unitOfWorkMock.Verify(u => u.ApprovalDelegationRepository.AddAsync(It.IsAny<ApprovalDelegation>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ValidateParallelApprovalRequirementsAsync_WithSufficientApprovals_ReturnsTrue()
    {
        // Arrange
        var parallelGroupId = "group1";
        var requestId = Guid.NewGuid();
        var minimumApprovals = 2;

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ParallelGroupId = parallelGroupId,
            MinimumApprovals = minimumApprovals
        };

        var parallelSteps = new List<RequestApprovalStep>
        {
            new RequestApprovalStep
            {
                RequestId = requestId,
                StepTemplate = stepTemplate,
                Status = ApprovalStepStatus.Approved
            },
            new RequestApprovalStep
            {
                RequestId = requestId,
                StepTemplate = stepTemplate,
                Status = ApprovalStepStatus.Approved
            },
            new RequestApprovalStep
            {
                RequestId = requestId,
                StepTemplate = stepTemplate,
                Status = ApprovalStepStatus.Pending
            }
        };

        _unitOfWorkMock.Setup(u => u.RequestApprovalStepRepository.GetAllAsync())
            .ReturnsAsync(parallelSteps);

        // Act
        var result = await _service.ValidateParallelApprovalRequirementsAsync(parallelGroupId, requestId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateParallelApprovalRequirementsAsync_WithInsufficientApprovals_ReturnsFalse()
    {
        // Arrange
        var parallelGroupId = "group1";
        var requestId = Guid.NewGuid();
        var minimumApprovals = 3;

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ParallelGroupId = parallelGroupId,
            MinimumApprovals = minimumApprovals
        };

        var parallelSteps = new List<RequestApprovalStep>
        {
            new RequestApprovalStep
            {
                RequestId = requestId,
                StepTemplate = stepTemplate,
                Status = ApprovalStepStatus.Approved
            },
            new RequestApprovalStep
            {
                RequestId = requestId,
                StepTemplate = stepTemplate,
                Status = ApprovalStepStatus.Pending
            }
        };

        _unitOfWorkMock.Setup(u => u.RequestApprovalStepRepository.GetAllAsync())
            .ReturnsAsync(parallelSteps);

        // Act
        var result = await _service.ValidateParallelApprovalRequirementsAsync(parallelGroupId, requestId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetEffectiveApproverAsync_WithActiveDelegation_ReturnsDelegatedUser()
    {
        // Arrange
        var submitter = new User { Id = Guid.NewGuid(), FullName = "Submitter" };
        var primaryApprover = new User { Id = Guid.NewGuid(), FullName = "Primary Approver" };
        var delegatedApprover = new User { Id = Guid.NewGuid(), FullName = "Delegated Approver" };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.SpecificUser,
            SpecificUser = primaryApprover
        };

        var activeDelegation = new ApprovalDelegation
        {
            FromUserId = primaryApprover.Id,
            ToUserId = delegatedApprover.Id,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        _unitOfWorkMock.Setup(u => u.ApprovalDelegationRepository.GetAllAsync())
            .ReturnsAsync(new[] { activeDelegation });
        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(delegatedApprover.Id))
            .ReturnsAsync(delegatedApprover);

        // Act
        var result = await _service.GetEffectiveApproverAsync(stepTemplate, submitter, true);

        // Assert
        Assert.Equal(delegatedApprover.Id, result?.Id);
    }
}