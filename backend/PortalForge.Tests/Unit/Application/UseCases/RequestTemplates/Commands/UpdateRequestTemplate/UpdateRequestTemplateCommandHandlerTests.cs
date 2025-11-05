using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.RequestTemplates.Commands.UpdateRequestTemplate;
using PortalForge.Application.UseCases.RequestTemplates.DTOs;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using Xunit;

namespace PortalForge.Tests.Unit.Application.UseCases.RequestTemplates.Commands.UpdateRequestTemplate;

public class UpdateRequestTemplateCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UpdateRequestTemplateCommandHandler>> _loggerMock;
    private readonly UpdateRequestTemplateCommandHandler _handler;

    public UpdateRequestTemplateCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UpdateRequestTemplateCommandHandler>>();

        _handler = new UpdateRequestTemplateCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesTemplateSuccessfully()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var command = new UpdateRequestTemplateCommand
        {
            Id = templateId,
            Name = "Updated Template Name",
            Description = "Updated Description",
            Icon = "Laptop",
            Category = "Hardware",
            EstimatedProcessingDays = 10,
            IsActive = false
        };

        var existingTemplate = new RequestTemplate
        {
            Id = templateId,
            Name = "Original Template",
            Description = "Original Description",
            Icon = "FileText",
            Category = "Software",
            IsActive = true,
            EstimatedProcessingDays = 7,
            Fields = new List<RequestTemplateField>(),
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>(),
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.GetByIdAsync(templateId))
            .ReturnsAsync(existingTemplate);

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.UpdateAsync(It.IsAny<RequestTemplate>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        existingTemplate.Name.Should().Be("Updated Template Name");
        existingTemplate.Description.Should().Be("Updated Description");
        existingTemplate.Icon.Should().Be("Laptop");
        existingTemplate.Category.Should().Be("Hardware");
        existingTemplate.EstimatedProcessingDays.Should().Be(10);
        existingTemplate.IsActive.Should().BeFalse();
        existingTemplate.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _unitOfWorkMock.Verify(x => x.RequestTemplateRepository.GetByIdAsync(templateId), Times.Once);
        _unitOfWorkMock.Verify(x => x.RequestTemplateRepository.UpdateAsync(existingTemplate), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_TemplateNotFound_ReturnsFailureResult()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var command = new UpdateRequestTemplateCommand
        {
            Id = templateId,
            Name = "Updated Template Name",
            Description = "Updated Description"
        };

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.GetByIdAsync(templateId))
            .ReturnsAsync((RequestTemplate?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");

        _unitOfWorkMock.Verify(x => x.RequestTemplateRepository.GetByIdAsync(templateId), Times.Once);
        _unitOfWorkMock.Verify(x => x.RequestTemplateRepository.UpdateAsync(It.IsAny<RequestTemplate>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdateFields_ReplacesExistingFields()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var command = new UpdateRequestTemplateCommand
        {
            Id = templateId,
            Name = "Template with Fields",
            Description = "Description",
            Fields = new List<RequestTemplateFieldDto>
            {
                new RequestTemplateFieldDto 
                { 
                    Label = "New Field 1", 
                    FieldType = "Text", 
                    Order = 1 
                },
                new RequestTemplateFieldDto 
                { 
                    Label = "New Field 2", 
                    FieldType = "Number", 
                    Order = 2 
                }
            }
        };

        var existingTemplate = new RequestTemplate
        {
            Id = templateId,
            Name = "Template with Fields",
            Description = "Description",
            Icon = "FileText",
            Category = "Test",
            Fields = new List<RequestTemplateField>
            {
                new RequestTemplateField 
                { 
                    Id = Guid.NewGuid(),
                    Label = "Old Field", 
                    FieldType = FieldType.Text, 
                    Order = 1 
                }
            },
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>(),
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.GetByIdAsync(templateId))
            .ReturnsAsync(existingTemplate);

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.UpdateAsync(It.IsAny<RequestTemplate>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        existingTemplate.Fields.Should().HaveCount(2);
        existingTemplate.Fields.Should().Contain(f => f.Label == "New Field 1");
        existingTemplate.Fields.Should().Contain(f => f.Label == "New Field 2");
        existingTemplate.Fields.Should().NotContain(f => f.Label == "Old Field");
    }

    [Fact]
    public async Task Handle_UpdateApprovalSteps_ReplacesExistingSteps()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var command = new UpdateRequestTemplateCommand
        {
            Id = templateId,
            Name = "Template with Approvals",
            Description = "Description",
            ApprovalStepTemplates = new List<RequestApprovalStepTemplateDto>
            {
                new RequestApprovalStepTemplateDto
                {
                    StepOrder = 1,
                    ApproverType = "DirectSupervisor",
                    RequiresQuiz = true
                }
            }
        };

        var existingTemplate = new RequestTemplate
        {
            Id = templateId,
            Name = "Template with Approvals",
            Description = "Description",
            Icon = "FileText",
            Category = "Test",
            Fields = new List<RequestTemplateField>(),
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>
            {
                new RequestApprovalStepTemplate
                {
                    Id = Guid.NewGuid(),
                    StepOrder = 1,
                    ApproverType = ApproverType.DirectSupervisor,
                    RequiresQuiz = false
                },
                new RequestApprovalStepTemplate
                {
                    Id = Guid.NewGuid(),
                    StepOrder = 2,
                    ApproverType = ApproverType.DirectSupervisor,
                    RequiresQuiz = false
                }
            },
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.GetByIdAsync(templateId))
            .ReturnsAsync(existingTemplate);

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.UpdateAsync(It.IsAny<RequestTemplate>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        existingTemplate.ApprovalStepTemplates.Should().HaveCount(1);
        existingTemplate.ApprovalStepTemplates.First().ApproverType.Should().Be(ApproverType.DirectSupervisor);
        existingTemplate.ApprovalStepTemplates.First().RequiresQuiz.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UpdateQuizQuestions_ReplacesExistingQuestions()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var command = new UpdateRequestTemplateCommand
        {
            Id = templateId,
            Name = "Template with Quiz",
            Description = "Description",
            PassingScore = 90,
            QuizQuestions = new List<QuizQuestionDto>
            {
                new QuizQuestionDto
                {
                    Question = "New Question?",
                    Options = "[{\"value\":\"a\",\"label\":\"Answer\",\"isCorrect\":true}]",
                    Order = 1
                }
            }
        };

        var existingTemplate = new RequestTemplate
        {
            Id = templateId,
            Name = "Template with Quiz",
            Description = "Description",
            Icon = "FileText",
            Category = "Test",
            PassingScore = 80,
            Fields = new List<RequestTemplateField>(),
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>(),
            QuizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion 
                { 
                    Id = Guid.NewGuid(),
                    Question = "Old Question?",
                    Options = "[]",
                    Order = 1 
                }
            },
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.GetByIdAsync(templateId))
            .ReturnsAsync(existingTemplate);

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.UpdateAsync(It.IsAny<RequestTemplate>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        existingTemplate.PassingScore.Should().Be(90);
        existingTemplate.QuizQuestions.Should().HaveCount(1);
        existingTemplate.QuizQuestions.First().Question.Should().Be("New Question?");
    }
}

