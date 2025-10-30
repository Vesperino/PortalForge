using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.RequestTemplates.Commands.CreateRequestTemplate;
using PortalForge.Application.UseCases.RequestTemplates.DTOs;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using Xunit;

namespace PortalForge.Tests.Unit.Application.UseCases.RequestTemplates.Commands.CreateRequestTemplate;

public class CreateRequestTemplateCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateRequestTemplateCommandHandler _handler;

    public CreateRequestTemplateCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateRequestTemplateCommandHandler(
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesTemplateSuccessfully()
    {
        // Arrange
        var createdById = Guid.NewGuid();
        var command = new CreateRequestTemplateCommand
        {
            Name = "Test Template",
            Description = "Test Description",
            Icon = "FileText",
            Category = "Hardware",
            DepartmentId = "IT",
            RequiresApproval = true,
            EstimatedProcessingDays = 7,
            CreatedById = createdById,
            Fields = new List<RequestTemplateFieldDto>
            {
                new RequestTemplateFieldDto
                {
                    Label = "Test Field",
                    FieldType = "Text",
                    IsRequired = true,
                    Order = 1
                }
            },
            ApprovalStepTemplates = new List<RequestApprovalStepTemplateDto>
            {
                new RequestApprovalStepTemplateDto
                {
                    StepOrder = 1,
                    ApproverType = "Role",
                    ApproverRole = "Manager",
                    RequiresQuiz = false
                }
            }
        };

        RequestTemplate? capturedTemplate = null;
        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.CreateAsync(It.IsAny<RequestTemplate>()))
            .Callback<RequestTemplate>(t => capturedTemplate = t)
            .ReturnsAsync((RequestTemplate t) => t.Id);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        capturedTemplate.Should().NotBeNull();
        capturedTemplate!.Name.Should().Be("Test Template");
        capturedTemplate.Description.Should().Be("Test Description");
        capturedTemplate.Icon.Should().Be("FileText");
        capturedTemplate.Category.Should().Be("Hardware");
        capturedTemplate.DepartmentId.Should().Be("IT");
        capturedTemplate.RequiresApproval.Should().BeTrue();
        capturedTemplate.EstimatedProcessingDays.Should().Be(7);
        capturedTemplate.CreatedById.Should().Be(createdById);
        capturedTemplate.Fields.Should().HaveCount(1);
        capturedTemplate.ApprovalStepTemplates.Should().HaveCount(1);

        _unitOfWorkMock.Verify(x => x.RequestTemplateRepository.CreateAsync(It.IsAny<RequestTemplate>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithQuizQuestions_CreatesTemplateWithQuiz()
    {
        // Arrange
        var createdById = Guid.NewGuid();
        var command = new CreateRequestTemplateCommand
        {
            Name = "Test Template with Quiz",
            Description = "Test Description",
            Icon = "FileText",
            Category = "Security",
            RequiresApproval = true,
            EstimatedProcessingDays = 7,
            PassingScore = 80,
            CreatedById = createdById,
            Fields = new List<RequestTemplateFieldDto>(),
            ApprovalStepTemplates = new List<RequestApprovalStepTemplateDto>
            {
                new RequestApprovalStepTemplateDto
                {
                    StepOrder = 1,
                    ApproverType = "Role",
                    ApproverRole = "Manager",
                    RequiresQuiz = true
                }
            },
            QuizQuestions = new List<QuizQuestionDto>
            {
                new QuizQuestionDto
                {
                    Question = "Test Question?",
                    Options = "[{\"value\":\"a\",\"label\":\"Answer A\",\"isCorrect\":true}]",
                    Order = 1
                }
            }
        };

        RequestTemplate? capturedTemplate = null;
        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.CreateAsync(It.IsAny<RequestTemplate>()))
            .Callback<RequestTemplate>(t => capturedTemplate = t)
            .ReturnsAsync((RequestTemplate t) => t.Id);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        capturedTemplate.Should().NotBeNull();
        capturedTemplate!.PassingScore.Should().Be(80);
        capturedTemplate.QuizQuestions.Should().HaveCount(1);
        capturedTemplate.QuizQuestions.First().Question.Should().Be("Test Question?");
    }

    [Fact]
    public async Task Handle_MinimalTemplate_CreatesWithDefaults()
    {
        // Arrange
        var createdById = Guid.NewGuid();
        var command = new CreateRequestTemplateCommand
        {
            Name = "Minimal Template",
            Description = "Minimal Description",
            Icon = "FileText",
            Category = "Testing",
            CreatedById = createdById,
            Fields = new List<RequestTemplateFieldDto>(),
            ApprovalStepTemplates = new List<RequestApprovalStepTemplateDto>()
        };

        RequestTemplate? capturedTemplate = null;
        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.CreateAsync(It.IsAny<RequestTemplate>()))
            .Callback<RequestTemplate>(t => capturedTemplate = t)
            .ReturnsAsync((RequestTemplate t) => t.Id);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        capturedTemplate.Should().NotBeNull();
        capturedTemplate!.IsActive.Should().BeTrue(); // Default value
        capturedTemplate.RequiresApproval.Should().BeTrue(); // Default value
        capturedTemplate.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }


    [Fact]
    public async Task Handle_WithMultipleFields_MaintainsFieldOrder()
    {
        // Arrange
        var createdById = Guid.NewGuid();
        var command = new CreateRequestTemplateCommand
        {
            Name = "Test Template",
            Description = "Test Description",
            Icon = "FileText",
            Category = "Hardware",
            CreatedById = createdById,
            Fields = new List<RequestTemplateFieldDto>
            {
                new RequestTemplateFieldDto { Label = "Field 1", FieldType = "Text", Order = 1 },
                new RequestTemplateFieldDto { Label = "Field 2", FieldType = "Number", Order = 2 },
                new RequestTemplateFieldDto { Label = "Field 3", FieldType = "Date", Order = 3 }
            },
            ApprovalStepTemplates = new List<RequestApprovalStepTemplateDto>()
        };

        RequestTemplate? capturedTemplate = null;
        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.CreateAsync(It.IsAny<RequestTemplate>()))
            .Callback<RequestTemplate>(t => capturedTemplate = t)
            .ReturnsAsync((RequestTemplate t) => t.Id);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedTemplate.Should().NotBeNull();
        capturedTemplate!.Fields.Should().HaveCount(3);
        capturedTemplate.Fields.Should().BeInAscendingOrder(f => f.Order);
        capturedTemplate.Fields.ElementAt(0).Label.Should().Be("Field 1");
        capturedTemplate.Fields.ElementAt(1).Label.Should().Be("Field 2");
        capturedTemplate.Fields.ElementAt(2).Label.Should().Be("Field 3");
    }

    [Fact]
    public async Task Handle_WithMultipleApprovalSteps_MaintainsStepOrder()
    {
        // Arrange
        var createdById = Guid.NewGuid();
        var command = new CreateRequestTemplateCommand
        {
            Name = "Test Template",
            Description = "Test Description",
            Icon = "FileText",
            Category = "Hardware",
            CreatedById = createdById,
            Fields = new List<RequestTemplateFieldDto>(),
            ApprovalStepTemplates = new List<RequestApprovalStepTemplateDto>
            {
                new RequestApprovalStepTemplateDto 
                { 
                    StepOrder = 1,
                    ApproverType = "Role",
                    ApproverRole = "Manager",
                    RequiresQuiz = false 
                },
                new RequestApprovalStepTemplateDto 
                { 
                    StepOrder = 2,
                    ApproverType = "Role",
                    ApproverRole = "Director",
                    RequiresQuiz = false 
                },
                new RequestApprovalStepTemplateDto 
                { 
                    StepOrder = 3,
                    ApproverType = "Role",
                    ApproverRole = "Employee",
                    RequiresQuiz = true 
                }
            }
        };

        RequestTemplate? capturedTemplate = null;
        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.CreateAsync(It.IsAny<RequestTemplate>()))
            .Callback<RequestTemplate>(t => capturedTemplate = t)
            .ReturnsAsync((RequestTemplate t) => t.Id);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedTemplate.Should().NotBeNull();
        capturedTemplate!.ApprovalStepTemplates.Should().HaveCount(3);
        capturedTemplate.ApprovalStepTemplates.Should().BeInAscendingOrder(s => s.StepOrder);
        capturedTemplate.ApprovalStepTemplates.ElementAt(0).ApproverRole.Should().Be(DepartmentRole.Manager);
        capturedTemplate.ApprovalStepTemplates.ElementAt(1).ApproverRole.Should().Be(DepartmentRole.Director);
        capturedTemplate.ApprovalStepTemplates.ElementAt(2).ApproverRole.Should().Be(DepartmentRole.Employee);
        capturedTemplate.ApprovalStepTemplates.ElementAt(2).RequiresQuiz.Should().BeTrue();
    }
}

