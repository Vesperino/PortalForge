using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Application.UseCases.Requests.Commands.SubmitRequest;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Requests;

public class SubmitRequestCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRequestTemplateRepository> _mockTemplateRepo;
    private readonly Mock<IRequestRepository> _mockRequestRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IRequestRoutingService> _mockRoutingService;
    private readonly Mock<IVacationCalculationService> _mockVacationService;
    private readonly Mock<ILogger<SubmitRequestCommandHandler>> _mockLogger;
    private readonly SubmitRequestCommandHandler _handler;

    public SubmitRequestCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTemplateRepo = new Mock<IRequestTemplateRepository>();
        _mockRequestRepo = new Mock<IRequestRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockRoutingService = new Mock<IRequestRoutingService>();
        _mockVacationService = new Mock<IVacationCalculationService>();
        _mockLogger = new Mock<ILogger<SubmitRequestCommandHandler>>();

        _mockUnitOfWork.Setup(u => u.RequestTemplateRepository).Returns(_mockTemplateRepo.Object);
        _mockUnitOfWork.Setup(u => u.RequestRepository).Returns(_mockRequestRepo.Object);
        _mockUnitOfWork.Setup(u => u.UserRepository).Returns(_mockUserRepo.Object);

        _handler = new SubmitRequestCommandHandler(
            _mockUnitOfWork.Object,
            _mockNotificationService.Object,
            _mockRoutingService.Object,
            _mockVacationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesRequestSuccessfully()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var submitterId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();

        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "IT Equipment Request",
            RequiresApproval = true,
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>
            {
                new RequestApprovalStepTemplate
                {
                    Id = Guid.NewGuid(),
                    StepOrder = 1,
                    ApproverRole = DepartmentRole.Manager,
                    RequiresQuiz = false
                }
            }
        };

        var supervisor = new User
        {
            Id = supervisorId,
            FirstName = "John",
            LastName = "Manager",
            DepartmentRole = DepartmentRole.Manager
        };

        var submitter = new User
        {
            Id = submitterId,
            FirstName = "Jane",
            LastName = "Employee",
            Supervisor = supervisor,
            SupervisorId = supervisorId
        };

        _mockTemplateRepo.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        _mockUserRepo.Setup(r => r.GetByIdAsync(submitterId))
            .ReturnsAsync(submitter);
        _mockRequestRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Request>());

        var command = new SubmitRequestCommand
        {
            RequestTemplateId = templateId,
            SubmittedById = submitterId,
            Priority = "Standard",
            FormData = "{\"equipment\":\"Laptop\"}"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Contains("REQ-", result.RequestNumber);
        Assert.Equal("Request submitted successfully", result.Message);
        
        _mockRequestRepo.Verify(r => r.CreateAsync(It.IsAny<Request>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_TemplateNotFound_ThrowsException()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var submitterId = Guid.NewGuid();

        _mockTemplateRepo.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync((RequestTemplate?)null);

        var command = new SubmitRequestCommand
        {
            RequestTemplateId = templateId,
            SubmittedById = submitterId
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NoApprovalRequired_CreatesApprovedRequest()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var submitterId = Guid.NewGuid();

        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "Simple Request",
            RequiresApproval = false,
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>()
        };

        var submitter = new User
        {
            Id = submitterId,
            FirstName = "Jane",
            LastName = "Employee"
        };

        _mockTemplateRepo.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        _mockUserRepo.Setup(r => r.GetByIdAsync(submitterId))
            .ReturnsAsync(submitter);
        _mockRequestRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Request>());

        var command = new SubmitRequestCommand
        {
            RequestTemplateId = templateId,
            SubmittedById = submitterId,
            Priority = "Standard",
            FormData = "{}"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        _mockRequestRepo.Verify(r => r.CreateAsync(
            It.Is<Request>(req => req.Status == RequestStatus.Approved)),
            Times.Once);
    }

    #region Auto-Approval Tests (TDD for Task 2.2)

    [Fact]
    public async Task Handle_PresidentSubmitsRequest_AutoApprovesStep()
    {
        // Arrange - President has no supervisor, so step should be auto-approved
        var templateId = Guid.NewGuid();
        var presidentId = Guid.NewGuid();

        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "Vacation Request",
            RequiresApproval = true,
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>
            {
                new RequestApprovalStepTemplate
                {
                    Id = Guid.NewGuid(),
                    StepOrder = 1,
                    ApproverType = ApproverType.Role,
                    ApproverRole = DepartmentRole.Director,
                    RequiresQuiz = false
                }
            }
        };

        var president = new User
        {
            Id = presidentId,
            FirstName = "John",
            LastName = "President",
            DepartmentRole = DepartmentRole.President,
            Supervisor = null // No supervisor!
        };

        _mockTemplateRepo.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        _mockUserRepo.Setup(r => r.GetByIdAsync(presidentId))
            .ReturnsAsync(president);
        _mockRequestRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Request>());

        // RoutingService returns null (no approver found)
        _mockRoutingService.Setup(s => s.ResolveApproverAsync(
                It.IsAny<RequestApprovalStepTemplate>(),
                It.IsAny<User>()))
            .ReturnsAsync((User?)null);

        var command = new SubmitRequestCommand
        {
            RequestTemplateId = templateId,
            SubmittedById = presidentId,
            Priority = "Standard",
            FormData = "{\"startDate\":\"2025-11-01\",\"endDate\":\"2025-11-10\"}"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);

        _mockRequestRepo.Verify(r => r.CreateAsync(
            It.Is<Request>(req =>
                req.ApprovalSteps.Count == 1 &&
                req.ApprovalSteps.First().Status == ApprovalStepStatus.Approved &&
                req.ApprovalSteps.First().Comment.Contains("Auto-approved")
            )),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmployeeWithSupervisor_CreatesNormalApprovalStep()
    {
        // Arrange - Regular employee with supervisor, normal approval flow
        var templateId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "Equipment Request",
            RequiresApproval = true,
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>
            {
                new RequestApprovalStepTemplate
                {
                    Id = Guid.NewGuid(),
                    StepOrder = 1,
                    ApproverType = ApproverType.DirectSupervisor,
                    RequiresQuiz = false
                }
            }
        };

        var manager = new User
        {
            Id = managerId,
            FirstName = "Jane",
            LastName = "Manager",
            DepartmentRole = DepartmentRole.Manager
        };

        var employee = new User
        {
            Id = employeeId,
            FirstName = "Bob",
            LastName = "Employee",
            DepartmentRole = DepartmentRole.Employee,
            Supervisor = manager,
            SupervisorId = managerId
        };

        _mockTemplateRepo.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        _mockUserRepo.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);
        _mockRequestRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Request>());

        // RoutingService returns manager (normal approval)
        _mockRoutingService.Setup(s => s.ResolveApproverAsync(
                It.IsAny<RequestApprovalStepTemplate>(),
                It.IsAny<User>()))
            .ReturnsAsync(manager);

        var command = new SubmitRequestCommand
        {
            RequestTemplateId = templateId,
            SubmittedById = employeeId,
            Priority = "Standard",
            FormData = "{}"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _mockRequestRepo.Verify(r => r.CreateAsync(
            It.Is<Request>(req =>
                req.ApprovalSteps.Count == 1 &&
                req.ApprovalSteps.First().ApproverId == managerId &&
                req.ApprovalSteps.First().Status == ApprovalStepStatus.InReview
            )),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MultiStepWithMixedAutoApproval_HandlesCorrectly()
    {
        // Arrange - 2 steps: first auto-approved, second normal
        var templateId = Guid.NewGuid();
        var directorId = Guid.NewGuid();
        var vpId = Guid.NewGuid();

        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "Budget Request",
            RequiresApproval = true,
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>
            {
                new RequestApprovalStepTemplate
                {
                    Id = Guid.NewGuid(),
                    StepOrder = 1,
                    ApproverType = ApproverType.Role,
                    ApproverRole = DepartmentRole.VP,
                    RequiresQuiz = false
                },
                new RequestApprovalStepTemplate
                {
                    Id = Guid.NewGuid(),
                    StepOrder = 2,
                    ApproverType = ApproverType.SpecificUser,
                    SpecificUserId = vpId,
                    RequiresQuiz = false
                }
            }
        };

        var vp = new User
        {
            Id = vpId,
            FirstName = "VP",
            LastName = "Finance",
            DepartmentRole = DepartmentRole.VP
        };

        var director = new User
        {
            Id = directorId,
            FirstName = "Director",
            LastName = "Operations",
            DepartmentRole = DepartmentRole.Director,
            Supervisor = null // No VP supervisor - first step auto-approves
        };

        _mockTemplateRepo.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        _mockUserRepo.Setup(r => r.GetByIdAsync(directorId))
            .ReturnsAsync(director);
        _mockRequestRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Request>());

        // First step: no approver (auto-approve), second step: specific user
        _mockRoutingService.Setup(s => s.ResolveApproverAsync(
                It.Is<RequestApprovalStepTemplate>(t => t.StepOrder == 1),
                It.IsAny<User>()))
            .ReturnsAsync((User?)null); // Auto-approve

        _mockRoutingService.Setup(s => s.ResolveApproverAsync(
                It.Is<RequestApprovalStepTemplate>(t => t.StepOrder == 2),
                It.IsAny<User>()))
            .ReturnsAsync(vp); // Normal approval

        var command = new SubmitRequestCommand
        {
            RequestTemplateId = templateId,
            SubmittedById = directorId,
            Priority = "Urgent",
            FormData = "{\"amount\":50000}"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _mockRequestRepo.Verify(r => r.CreateAsync(
            It.Is<Request>(req =>
                req.ApprovalSteps.Count == 2 &&
                req.ApprovalSteps.First(s => s.StepOrder == 1).Status == ApprovalStepStatus.Approved &&
                req.ApprovalSteps.First(s => s.StepOrder == 2).ApproverId == vpId &&
                req.ApprovalSteps.First(s => s.StepOrder == 2).Status == ApprovalStepStatus.InReview
            )),
            Times.Once);
    }

    #endregion
}

