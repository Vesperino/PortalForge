using FluentAssertions;
using Moq;
using Xunit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Requests.Queries.GetMyRequests;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Tests.Unit.Requests;

public class GetMyRequestsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRequestRepository> _mockRequestRepo;
    private readonly GetMyRequestsQueryHandler _handler;

    public GetMyRequestsQueryHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRequestRepo = new Mock<IRequestRepository>();

        _mockUnitOfWork.Setup(u => u.RequestRepository).Returns(_mockRequestRepo.Object);

        _handler = new GetMyRequestsQueryHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_BasicQuery_ReturnsAllUserRequests()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requests = CreateTestRequests(userId);

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);

        var query = new GetMyRequestsQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Requests.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.FilterSummary.TotalRequests.Should().Be(5);
    }

    [Fact]
    public async Task Handle_SearchTermFilter_FiltersCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requests = CreateTestRequests(userId);

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);

        var query = new GetMyRequestsQuery 
        { 
            UserId = userId,
            SearchTerm = "laptop" // Should match requests with laptop in form data
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Requests.Should().HaveCount(2); // Two requests have "laptop" in form data
    }

    [Fact]
    public async Task Handle_StatusFilter_FiltersCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requests = CreateTestRequests(userId);

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);

        var query = new GetMyRequestsQuery 
        { 
            UserId = userId,
            StatusFilter = new List<RequestStatus> { RequestStatus.Approved }
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Requests.Should().HaveCount(2); // Two approved requests
    }

    [Fact]
    public async Task Handle_DateRangeFilter_FiltersCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requests = CreateTestRequests(userId);

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);

        var query = new GetMyRequestsQuery 
        { 
            UserId = userId,
            SubmittedAfter = DateTime.UtcNow.AddDays(-15),
            SubmittedBefore = DateTime.UtcNow.AddDays(-5)
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Requests.Should().HaveCount(2); // Two requests in date range
    }

    [Fact]
    public async Task Handle_PaginationFilter_PaginatesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requests = CreateTestRequests(userId);

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);

        var query = new GetMyRequestsQuery 
        { 
            UserId = userId,
            PageNumber = 2,
            PageSize = 2
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Requests.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SortByPriority_SortsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requests = CreateTestRequests(userId);

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);

        var query = new GetMyRequestsQuery 
        { 
            UserId = userId,
            SortBy = "priority",
            SortDirection = "DESC"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Requests.Should().NotBeEmpty();
        // First request should have highest priority
        result.Requests.First().Priority.Should().Be("Urgent");
    }

    [Fact]
    public async Task Handle_ClonedFilter_FiltersClonedRequests()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requests = CreateTestRequests(userId);

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);

        var query = new GetMyRequestsQuery 
        { 
            UserId = userId,
            IsCloned = true
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Requests.Should().HaveCount(1); // One cloned request
    }

    [Fact]
    public async Task Handle_LeaveTypeFilter_FiltersVacationRequests()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requests = CreateTestRequests(userId);

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);

        var query = new GetMyRequestsQuery 
        { 
            UserId = userId,
            LeaveTypeFilter = new List<LeaveType> { LeaveType.Annual }
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Requests.Should().HaveCount(1); // One annual leave request
    }

    [Fact]
    public async Task Handle_FilterSummary_CalculatesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requests = CreateTestRequests(userId);

        _mockRequestRepo.Setup(r => r.GetBySubmitterAsync(userId))
            .ReturnsAsync(requests);

        var query = new GetMyRequestsQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.FilterSummary.Should().NotBeNull();
        result.FilterSummary.TotalRequests.Should().Be(5);
        result.FilterSummary.ApprovedRequests.Should().Be(2);
        result.FilterSummary.InReviewRequests.Should().Be(2);
        result.FilterSummary.DraftRequests.Should().Be(1);
        result.FilterSummary.ClonedRequests.Should().Be(1);
        result.FilterSummary.TemplateRequests.Should().Be(1);
    }

    private List<Request> CreateTestRequests(Guid userId)
    {
        var templateId = Guid.NewGuid();
        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "IT Equipment Request",
            Icon = "laptop"
        };

        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe"
        };

        return new List<Request>
        {
            new Request
            {
                Id = Guid.NewGuid(),
                RequestNumber = "REQ-2024-0001",
                RequestTemplateId = templateId,
                RequestTemplate = template,
                SubmittedById = userId,
                SubmittedBy = user,
                SubmittedAt = DateTime.UtcNow.AddDays(-30),
                Priority = RequestPriority.Standard,
                FormData = "{\"equipment\":\"laptop\",\"reason\":\"development\"}",
                Status = RequestStatus.Approved,
                CompletedAt = DateTime.UtcNow.AddDays(-25),
                ApprovalSteps = new List<RequestApprovalStep>()
            },
            new Request
            {
                Id = Guid.NewGuid(),
                RequestNumber = "REQ-2024-0002",
                RequestTemplateId = templateId,
                RequestTemplate = template,
                SubmittedById = userId,
                SubmittedBy = user,
                SubmittedAt = DateTime.UtcNow.AddDays(-20),
                Priority = RequestPriority.Urgent,
                FormData = "{\"equipment\":\"monitor\",\"reason\":\"productivity\"}",
                Status = RequestStatus.InReview,
                ApprovalSteps = new List<RequestApprovalStep>()
            },
            new Request
            {
                Id = Guid.NewGuid(),
                RequestNumber = "REQ-2024-0003",
                RequestTemplateId = templateId,
                RequestTemplate = template,
                SubmittedById = userId,
                SubmittedBy = user,
                SubmittedAt = DateTime.UtcNow.AddDays(-10),
                Priority = RequestPriority.Urgent,
                FormData = "{\"equipment\":\"laptop\",\"reason\":\"urgent project\"}",
                Status = RequestStatus.InReview,
                LeaveType = LeaveType.Annual,
                ApprovalSteps = new List<RequestApprovalStep>()
            },
            new Request
            {
                Id = Guid.NewGuid(),
                RequestNumber = "REQ-2024-0004",
                RequestTemplateId = templateId,
                RequestTemplate = template,
                SubmittedById = userId,
                SubmittedBy = user,
                SubmittedAt = DateTime.UtcNow.AddDays(-5),
                Priority = RequestPriority.Standard,
                FormData = "{\"equipment\":\"keyboard\",\"reason\":\"replacement\"}",
                Status = RequestStatus.Draft,
                ClonedFromId = Guid.NewGuid(), // This is a cloned request
                ApprovalSteps = new List<RequestApprovalStep>()
            },
            new Request
            {
                Id = Guid.NewGuid(),
                RequestNumber = "TMPL-2024-0001",
                RequestTemplateId = templateId,
                RequestTemplate = template,
                SubmittedById = userId,
                SubmittedBy = user,
                SubmittedAt = DateTime.UtcNow.AddDays(-2),
                Priority = RequestPriority.Standard,
                FormData = "{\"equipment\":\"standard setup\"}",
                Status = RequestStatus.Approved,
                IsTemplate = true, // This is a template
                CompletedAt = DateTime.UtcNow.AddDays(-1),
                ApprovalSteps = new List<RequestApprovalStep>()
            }
        };
    }
}