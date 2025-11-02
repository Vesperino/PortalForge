using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.Admin.Commands.UpdateUser;
using PortalForge.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace PortalForge.Tests.Unit.Admin;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<UpdateUserCommandHandler>> _mockLogger;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdateUserCommandHandler>>();

        _handler = new UpdateUserCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_AssignUserToDepartment_SetsDepartmentIdAndName()
    {
        var userId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();

        var existingUser = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Department = "Old Department",
            DepartmentId = null,
            Position = "Developer",
            Role = UserRole.Employee,
            IsActive = true
        };

        var department = new Department
        {
            Id = departmentId,
            Name = "IT Department",
            Description = "IT Department",
            IsActive = true
        };

        _mockUnitOfWork.Setup(u => u.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.GetByIdAsync(departmentId))
            .ReturnsAsync(department);

        _mockUnitOfWork.Setup(u => u.UserRoleGroupRepository.DeleteByUserIdAsync(userId))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.PositionRepository.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((Position?)null);

        _mockUnitOfWork.Setup(u => u.AuditLogRepository.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(Guid.NewGuid());

        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "Old Department",
            DepartmentId = departmentId,
            Position = "Developer",
            PhoneNumber = null,
            Role = "Employee",
            RoleGroupIds = new List<Guid>(),
            IsActive = true,
            UpdatedBy = updatedBy
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);

        _mockUnitOfWork.Verify(u => u.UserRepository.UpdateAsync(
            It.Is<User>(user =>
                user.Id == userId &&
                user.DepartmentId == departmentId &&
                user.Department == "IT Department"
            )), Times.Once);

        _mockUnitOfWork.Verify(u => u.DepartmentRepository.GetByIdAsync(departmentId), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.UserRepository.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT",
            DepartmentId = null,
            Position = "Developer",
            Role = "Employee",
            RoleGroupIds = new List<Guid>(),
            IsActive = true,
            UpdatedBy = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
