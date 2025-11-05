using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Application.Services;

public class ApprovalDelegationTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<EnhancedRequestRoutingService>> _loggerMock;
    private readonly EnhancedRequestRoutingService _service;

    public ApprovalDelegationTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<EnhancedRequestRoutingService>>();
        _service = new EnhancedRequestRoutingService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SetApprovalDelegationAsync_WithValidParameters_CreatesDelegation()
    {
        // Arrange
        var fromUserId = Guid.NewGuid();
        var toUserId = Guid.NewGuid();
        var fromUser = new User { Id = fromUserId, FullName = "From User", IsActive = true };
        var toUser = new User { Id = toUserId, FullName = "To User", IsActive = true };
        var until = DateTime.UtcNow.AddDays(7);
        var reason = "Vacation delegation";

        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(fromUserId))
            .ReturnsAsync(fromUser);
        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(toUserId))
            .ReturnsAsync(toUser);

        // Act
        var result = await _service.SetApprovalDelegationAsync(fromUserId, toUserId, until, reason);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fromUserId, result.FromUserId);
        Assert.Equal(toUserId, result.ToUserId);
        Assert.Equal(until, result.EndDate);
        Assert.Equal(reason, result.Reason);
        Assert.True(result.IsActive);
        Assert.True(result.StartDate <= DateTime.UtcNow);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);

        _unitOfWorkMock.Verify(u => u.ApprovalDelegationRepository.AddAsync(It.IsAny<ApprovalDelegation>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SetApprovalDelegationAsync_WithNonExistentFromUser_ThrowsException()
    {
        // Arrange
        var fromUserId = Guid.NewGuid();
        var toUserId = Guid.NewGuid();
        var toUser = new User { Id = toUserId, FullName = "To User", IsActive = true };

        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(fromUserId))
            .ReturnsAsync((User?)null);
        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(toUserId))
            .ReturnsAsync(toUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SetApprovalDelegationAsync(fromUserId, toUserId, null));
        
        Assert.Contains($"User {fromUserId} not found", exception.Message);
    }

    [Fact]
    public async Task SetApprovalDelegationAsync_WithNonExistentToUser_ThrowsException()
    {
        // Arrange
        var fromUserId = Guid.NewGuid();
        var toUserId = Guid.NewGuid();
        var fromUser = new User { Id = fromUserId, FullName = "From User", IsActive = true };

        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(fromUserId))
            .ReturnsAsync(fromUser);
        _unitOfWorkMock.Setup(u => u.UserRepository.GetByIdAsync(toUserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SetApprovalDelegationAsync(fromUserId, toUserId, null));
        
        Assert.Contains($"User {toUserId} not found", exception.Message);
    }

    [Fact]
    public async Task RemoveApprovalDelegationAsync_WithExistingDelegation_DeactivatesDelegation()
    {
        // Arrange
        var delegationId = Guid.NewGuid();
        var delegation = new ApprovalDelegation
        {
            Id = delegationId,
            IsActive = true,
            FromUserId = Guid.NewGuid(),
            ToUserId = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(u => u.ApprovalDelegationRepository.GetByIdAsync(delegationId))
            .ReturnsAsync(delegation);

        // Act
        var result = await _service.RemoveApprovalDelegationAsync(delegationId);

        // Assert
        Assert.True(result);
        Assert.False(delegation.IsActive);
        _unitOfWorkMock.Verify(u => u.ApprovalDelegationRepository.UpdateAsync(delegation), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveApprovalDelegationAsync_WithNonExistentDelegation_ReturnsFalse()
    {
        // Arrange
        var delegationId = Guid.NewGuid();

        _unitOfWorkMock.Setup(u => u.ApprovalDelegationRepository.GetByIdAsync(delegationId))
            .ReturnsAsync((ApprovalDelegation?)null);

        // Act
        var result = await _service.RemoveApprovalDelegationAsync(delegationId);

        // Assert
        Assert.False(result);
        _unitOfWorkMock.Verify(u => u.ApprovalDelegationRepository.UpdateAsync(It.IsAny<ApprovalDelegation>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetUserDelegationsAsync_WithActiveDelegations_ReturnsCorrectDelegations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentDate = DateTime.UtcNow;

        var delegationsFrom = new List<ApprovalDelegation>
        {
            new ApprovalDelegation
            {
                FromUserId = userId,
                ToUserId = Guid.NewGuid(),
                IsActive = true,
                StartDate = currentDate.AddDays(-1),
                EndDate = currentDate.AddDays(1)
            }
        };

        var delegationsTo = new List<ApprovalDelegation>
        {
            new ApprovalDelegation
            {
                FromUserId = Guid.NewGuid(),
                ToUserId = userId,
                IsActive = true,
                StartDate = currentDate.AddDays(-2),
                EndDate = null // Indefinite
            }
        };

        var allDelegations = delegationsFrom.Concat(delegationsTo).ToList();

        _unitOfWorkMock.Setup(u => u.ApprovalDelegationRepository.GetAllAsync())
            .ReturnsAsync(allDelegations);

        // Act
        var result = await _service.GetUserDelegationsAsync(userId);

        // Assert
        Assert.Single(result.DelegationsFrom);
        Assert.Single(result.DelegationsTo);
        Assert.Equal(userId, result.DelegationsFrom.First().FromUserId);
        Assert.Equal(userId, result.DelegationsTo.First().ToUserId);
    }

    [Fact]
    public async Task GetUserDelegationsAsync_WithExpiredDelegations_ExcludesExpiredDelegations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentDate = DateTime.UtcNow;

        var expiredDelegation = new ApprovalDelegation
        {
            FromUserId = userId,
            ToUserId = Guid.NewGuid(),
            IsActive = true,
            StartDate = currentDate.AddDays(-10),
            EndDate = currentDate.AddDays(-1) // Expired yesterday
        };

        var activeDelegation = new ApprovalDelegation
        {
            FromUserId = userId,
            ToUserId = Guid.NewGuid(),
            IsActive = true,
            StartDate = currentDate.AddDays(-1),
            EndDate = currentDate.AddDays(1) // Active
        };

        var allDelegations = new List<ApprovalDelegation> { expiredDelegation, activeDelegation };

        _unitOfWorkMock.Setup(u => u.ApprovalDelegationRepository.GetAllAsync())
            .ReturnsAsync(allDelegations);

        // Act
        var result = await _service.GetUserDelegationsAsync(userId);

        // Assert
        Assert.Single(result.DelegationsFrom);
        Assert.Equal(activeDelegation.Id, result.DelegationsFrom.First().Id);
    }

    [Fact]
    public async Task GetUserDelegationsAsync_WithInactiveDelegations_ExcludesInactiveDelegations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentDate = DateTime.UtcNow;

        var inactiveDelegation = new ApprovalDelegation
        {
            FromUserId = userId,
            ToUserId = Guid.NewGuid(),
            IsActive = false, // Inactive
            StartDate = currentDate.AddDays(-1),
            EndDate = currentDate.AddDays(1)
        };

        var activeDelegation = new ApprovalDelegation
        {
            FromUserId = userId,
            ToUserId = Guid.NewGuid(),
            IsActive = true,
            StartDate = currentDate.AddDays(-1),
            EndDate = currentDate.AddDays(1)
        };

        var allDelegations = new List<ApprovalDelegation> { inactiveDelegation, activeDelegation };

        _unitOfWorkMock.Setup(u => u.ApprovalDelegationRepository.GetAllAsync())
            .ReturnsAsync(allDelegations);

        // Act
        var result = await _service.GetUserDelegationsAsync(userId);

        // Assert
        Assert.Single(result.DelegationsFrom);
        Assert.Equal(activeDelegation.Id, result.DelegationsFrom.First().Id);
    }
}