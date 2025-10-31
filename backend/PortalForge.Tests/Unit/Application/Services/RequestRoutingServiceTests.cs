using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using Xunit;

namespace PortalForge.Tests.Unit.Application.Services;

/// <summary>
/// Unit tests for RequestRoutingService.
/// Tests all 6 ApproverType scenarios and edge cases.
/// </summary>
public class RequestRoutingServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<RequestRoutingService>> _loggerMock;
    private readonly IRequestRoutingService _service;

    public RequestRoutingServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<RequestRoutingService>>();
        _service = new RequestRoutingService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    #region ResolveByRole Tests

    [Fact]
    public async Task ResolveApprover_ByRole_FindsManager_WhenSubmitterHasManagerSupervisor()
    {
        // Arrange
        var manager = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Manager",
            DepartmentRole = DepartmentRole.Manager
        };

        var submitter = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Employee",
            DepartmentRole = DepartmentRole.Employee,
            SupervisorId = manager.Id,
            Supervisor = manager
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.Role,
            ApproverRole = DepartmentRole.Manager
        };

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, submitter);

        // Assert
        approver.Should().NotBeNull();
        approver!.Id.Should().Be(manager.Id);
        approver.DepartmentRole.Should().Be(DepartmentRole.Manager);
    }

    [Fact]
    public async Task ResolveApprover_ByRole_FindsDirector_WhenSubmitterHasDirectorInChain()
    {
        // Arrange
        var director = new User
        {
            Id = Guid.NewGuid(),
            DepartmentRole = DepartmentRole.Director
        };

        var manager = new User
        {
            Id = Guid.NewGuid(),
            DepartmentRole = DepartmentRole.Manager,
            SupervisorId = director.Id,
            Supervisor = director
        };

        var submitter = new User
        {
            Id = Guid.NewGuid(),
            DepartmentRole = DepartmentRole.Employee,
            SupervisorId = manager.Id,
            Supervisor = manager
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.Role,
            ApproverRole = DepartmentRole.Director
        };

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, submitter);

        // Assert
        approver.Should().NotBeNull();
        approver!.Id.Should().Be(director.Id);
        approver.DepartmentRole.Should().Be(DepartmentRole.Director);
    }

    [Fact]
    public async Task ResolveApprover_ByRole_ReturnsNull_WhenPresidentHasNoSupervisor()
    {
        // Arrange
        var president = new User
        {
            Id = Guid.NewGuid(),
            DepartmentRole = DepartmentRole.President,
            Supervisor = null
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.Role,
            ApproverRole = DepartmentRole.President
        };

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, president);

        // Assert
        approver.Should().BeNull(); // Triggers auto-approval
    }

    [Fact]
    public async Task ResolveApprover_ByRole_ReturnsNull_WhenNoSupervisorWithTargetRole()
    {
        // Arrange
        var teamLead = new User
        {
            Id = Guid.NewGuid(),
            DepartmentRole = DepartmentRole.TeamLead,
            Supervisor = null
        };

        var submitter = new User
        {
            Id = Guid.NewGuid(),
            DepartmentRole = DepartmentRole.Employee,
            SupervisorId = teamLead.Id,
            Supervisor = teamLead
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.Role,
            ApproverRole = DepartmentRole.Director // Requires Director but chain stops at TeamLead
        };

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, submitter);

        // Assert
        approver.Should().BeNull();
    }

    #endregion

    #region ResolveByDirectSupervisor Tests

    [Fact]
    public async Task ResolveApprover_ByDirectSupervisor_ReturnsImmediateSupervisor()
    {
        // Arrange
        var supervisor = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Direct",
            LastName = "Supervisor",
            DepartmentRole = DepartmentRole.Manager
        };

        var submitter = new User
        {
            Id = Guid.NewGuid(),
            DepartmentRole = DepartmentRole.Employee,
            SupervisorId = supervisor.Id,
            Supervisor = supervisor
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.DirectSupervisor
        };

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, submitter);

        // Assert
        approver.Should().NotBeNull();
        approver!.Id.Should().Be(supervisor.Id);
    }

    [Fact]
    public async Task ResolveApprover_ByDirectSupervisor_ReturnsNull_WhenNoSupervisor()
    {
        // Arrange
        var submitter = new User
        {
            Id = Guid.NewGuid(),
            DepartmentRole = DepartmentRole.President,
            Supervisor = null
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.DirectSupervisor
        };

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, submitter);

        // Assert
        approver.Should().BeNull();
    }

    #endregion

    #region ResolveBySpecificDepartment Tests

    [Fact]
    public async Task ResolveApprover_BySpecificDepartment_ReturnsDepartmentHead()
    {
        // Arrange
        var departmentHead = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Department",
            LastName = "Head"
        };

        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "HR Department",
            HeadOfDepartmentId = departmentHead.Id,
            HeadOfDepartment = departmentHead
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.SpecificDepartment,
            SpecificDepartmentId = department.Id,
            SpecificDepartment = department
        };

        var submitter = new User { Id = Guid.NewGuid() };

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(department.Id))
            .ReturnsAsync(department);

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, submitter);

        // Assert
        approver.Should().NotBeNull();
        approver!.Id.Should().Be(departmentHead.Id);
    }

    [Fact]
    public async Task ResolveApprover_BySpecificDepartment_ReturnsNull_WhenDepartmentHasNoHead()
    {
        // Arrange
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Headless Department",
            HeadOfDepartmentId = null,
            HeadOfDepartment = null
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.SpecificDepartment,
            SpecificDepartmentId = department.Id,
            SpecificDepartment = department
        };

        var submitter = new User { Id = Guid.NewGuid() };

        _unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(department.Id))
            .ReturnsAsync(department);

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, submitter);

        // Assert
        approver.Should().BeNull();
    }

    #endregion

    #region ResolveBySpecificUser Tests

    [Fact]
    public async Task ResolveApprover_BySpecificUser_ReturnsSpecificUser()
    {
        // Arrange
        var specificUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Specific",
            LastName = "Approver"
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.SpecificUser,
            SpecificUserId = specificUser.Id,
            SpecificUser = specificUser
        };

        var submitter = new User { Id = Guid.NewGuid() };

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, submitter);

        // Assert
        approver.Should().NotBeNull();
        approver!.Id.Should().Be(specificUser.Id);
    }

    #endregion

    #region ResolveByUserGroup Tests

    [Fact]
    public async Task ResolveApprover_ByUserGroup_ReturnsFirstUserFromGroup()
    {
        // Arrange
        var groupUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Group",
            LastName = "Member"
        };

        var roleGroup = new RoleGroup
        {
            Id = Guid.NewGuid(),
            Name = "HR Group"
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.UserGroup,
            ApproverGroupId = roleGroup.Id,
            ApproverGroup = roleGroup
        };

        var submitter = new User { Id = Guid.NewGuid() };

        _unitOfWorkMock.Setup(x => x.RoleGroupRepository.GetUsersInGroupAsync(roleGroup.Id))
            .ReturnsAsync(new List<User> { groupUser });

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, submitter);

        // Assert
        approver.Should().NotBeNull();
        approver!.Id.Should().Be(groupUser.Id);
    }

    [Fact]
    public async Task ResolveApprover_ByUserGroup_ReturnsNull_WhenGroupIsEmpty()
    {
        // Arrange
        var roleGroup = new RoleGroup
        {
            Id = Guid.NewGuid(),
            Name = "Empty Group"
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.UserGroup,
            ApproverGroupId = roleGroup.Id,
            ApproverGroup = roleGroup
        };

        var submitter = new User { Id = Guid.NewGuid() };

        _unitOfWorkMock.Setup(x => x.RoleGroupRepository.GetUsersInGroupAsync(roleGroup.Id))
            .ReturnsAsync(new List<User>());

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, submitter);

        // Assert
        approver.Should().BeNull();
    }

    #endregion

    #region ResolveBySubmitter Tests

    [Fact]
    public async Task ResolveApprover_BySubmitter_ReturnsSubmitter()
    {
        // Arrange
        var submitter = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Self",
            LastName = "Approver"
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.Submitter
        };

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, submitter);

        // Assert
        approver.Should().NotBeNull();
        approver!.Id.Should().Be(submitter.Id);
    }

    #endregion

    #region HasHigherSupervisorAsync Tests

    [Fact]
    public async Task HasHigherSupervisor_ReturnsTrue_ForEmployeeWithManager()
    {
        // Arrange
        var manager = new User { Id = Guid.NewGuid() };
        var employee = new User
        {
            Id = Guid.NewGuid(),
            SupervisorId = manager.Id,
            Supervisor = manager
        };

        // Act
        var result = await _service.HasHigherSupervisorAsync(employee);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasHigherSupervisor_ReturnsFalse_ForPresident()
    {
        // Arrange
        var president = new User
        {
            Id = Guid.NewGuid(),
            DepartmentRole = DepartmentRole.President,
            Supervisor = null
        };

        // Act
        var result = await _service.HasHigherSupervisorAsync(president);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
