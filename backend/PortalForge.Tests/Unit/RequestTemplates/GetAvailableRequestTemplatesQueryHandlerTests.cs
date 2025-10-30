using Moq;
using Xunit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.RequestTemplates.Queries.GetAvailableRequestTemplates;
using PortalForge.Domain.Entities;

namespace PortalForge.Tests.Unit.RequestTemplates;

public class GetAvailableRequestTemplatesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRequestTemplateRepository> _mockTemplateRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly GetAvailableRequestTemplatesQueryHandler _handler;

    public GetAvailableRequestTemplatesQueryHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTemplateRepo = new Mock<IRequestTemplateRepository>();
        _mockUserRepo = new Mock<IUserRepository>();

        _mockUnitOfWork.Setup(u => u.RequestTemplateRepository).Returns(_mockTemplateRepo.Object);
        _mockUnitOfWork.Setup(u => u.UserRepository).Returns(_mockUserRepo.Object);

        _handler = new GetAvailableRequestTemplatesQueryHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ValidUser_ReturnsAvailableTemplates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Department = "IT"
        };

        var templates = new List<RequestTemplate>
        {
            new RequestTemplate
            {
                Id = Guid.NewGuid(),
                Name = "IT Equipment",
                Description = "Request IT equipment",
                Icon = "laptop",
                Category = "Hardware",
                DepartmentId = "IT",
                IsActive = true,
                CreatedById = Guid.NewGuid(),
                CreatedBy = new User { FirstName = "Admin", LastName = "User" }
            },
            new RequestTemplate
            {
                Id = Guid.NewGuid(),
                Name = "General Request",
                Description = "General purpose request",
                Icon = "file",
                Category = "Other",
                DepartmentId = null, // Available for all
                IsActive = true,
                CreatedById = Guid.NewGuid(),
                CreatedBy = new User { FirstName = "Admin", LastName = "User" }
            }
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockTemplateRepo.Setup(r => r.GetAvailableForUserAsync("IT"))
            .ReturnsAsync(templates);

        var query = new GetAvailableRequestTemplatesQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Templates.Count);
        Assert.Contains(result.Templates, t => t.Name == "IT Equipment");
        Assert.Contains(result.Templates, t => t.Name == "General Request");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepo.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var query = new GetAvailableRequestTemplatesQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Templates);
    }

    [Fact]
    public async Task Handle_TemplatesHaveCorrectData_MapsPropertiesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Department = "IT"
        };

        var templates = new List<RequestTemplate>
        {
            new RequestTemplate
            {
                Id = templateId,
                Name = "Test Template",
                Description = "Test Description",
                Icon = "test-icon",
                Category = "Test Category",
                DepartmentId = "IT",
                IsActive = true,
                RequiresApproval = true,
                EstimatedProcessingDays = 5,
                PassingScore = 80,
                CreatedById = creatorId,
                CreatedBy = new User { FirstName = "Test", LastName = "Creator" },
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockTemplateRepo.Setup(r => r.GetAvailableForUserAsync("IT"))
            .ReturnsAsync(templates);

        var query = new GetAvailableRequestTemplatesQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var template = result.Templates.First();
        Assert.Equal(templateId, template.Id);
        Assert.Equal("Test Template", template.Name);
        Assert.Equal("Test Description", template.Description);
        Assert.Equal("test-icon", template.Icon);
        Assert.Equal("Test Category", template.Category);
        Assert.Equal("IT", template.DepartmentId);
        Assert.True(template.IsActive);
        Assert.True(template.RequiresApproval);
        Assert.Equal(5, template.EstimatedProcessingDays);
        Assert.Equal(80, template.PassingScore);
        Assert.Equal("Test Creator", template.CreatedByName);
    }
}

