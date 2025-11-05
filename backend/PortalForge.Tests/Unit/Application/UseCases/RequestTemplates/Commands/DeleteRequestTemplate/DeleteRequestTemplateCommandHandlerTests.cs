using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.RequestTemplates.Commands.DeleteRequestTemplate;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Application.UseCases.RequestTemplates.Commands.DeleteRequestTemplate;

public class DeleteRequestTemplateCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<DeleteRequestTemplateCommandHandler>> _loggerMock;
    private readonly DeleteRequestTemplateCommandHandler _handler;

    public DeleteRequestTemplateCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<DeleteRequestTemplateCommandHandler>>();

        _handler = new DeleteRequestTemplateCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_DeletesTemplateSuccessfully()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var deletedBy = Guid.NewGuid();
        var command = new DeleteRequestTemplateCommand
        {
            Id = templateId,
            DeletedBy = deletedBy
        };

        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "Test Template",
            Description = "Test Description",
            Category = "Test Category",
            Fields = new List<RequestTemplateField>(),
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>()
        };

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByTemplateIdAsync(templateId))
            .ReturnsAsync(new List<Request>());

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.DeleteAsync(templateId))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(Guid.NewGuid());

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Template deleted successfully");

        _unitOfWorkMock.Verify(x => x.RequestTemplateRepository.GetByIdAsync(templateId), Times.Once);
        _unitOfWorkMock.Verify(x => x.RequestRepository.GetByTemplateIdAsync(templateId), Times.Once);
        _unitOfWorkMock.Verify(x => x.RequestTemplateRepository.DeleteAsync(templateId), Times.Once);
        _unitOfWorkMock.Verify(x => x.AuditLogRepository.CreateAsync(It.Is<AuditLog>(a =>
            a.UserId == deletedBy &&
            a.Action == "DeleteRequestTemplate" &&
            a.EntityType == "RequestTemplate" &&
            a.EntityId == templateId.ToString()
        )), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_TemplateNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var command = new DeleteRequestTemplateCommand
        {
            Id = templateId,
            DeletedBy = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.GetByIdAsync(templateId))
            .ReturnsAsync((RequestTemplate?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Request template with ID {templateId} not found");

        _unitOfWorkMock.Verify(x => x.RequestTemplateRepository.GetByIdAsync(templateId), Times.Once);
        _unitOfWorkMock.Verify(x => x.RequestTemplateRepository.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }


    [Fact]
    public async Task Handle_ValidRequest_CreatesAuditLogWithCorrectData()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var deletedBy = Guid.NewGuid();
        var command = new DeleteRequestTemplateCommand
        {
            Id = templateId,
            DeletedBy = deletedBy
        };

        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "Test Template",
            Description = "Test Description",
            Category = "Test Category",
            Fields = new List<RequestTemplateField>
            {
                new RequestTemplateField { Id = Guid.NewGuid(), Label = "Field 1" },
                new RequestTemplateField { Id = Guid.NewGuid(), Label = "Field 2" }
            },
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>
            {
                new RequestApprovalStepTemplate { Id = Guid.NewGuid(), StepOrder = 1 }
            }
        };

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByTemplateIdAsync(templateId))
            .ReturnsAsync(new List<Request>());

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.DeleteAsync(templateId))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(Guid.NewGuid());

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.AuditLogRepository.CreateAsync(It.Is<AuditLog>(a =>
            a.UserId == deletedBy &&
            a.Action == "DeleteRequestTemplate" &&
            a.EntityType == "RequestTemplate" &&
            a.EntityId == templateId.ToString() &&
            a.OldValue!.Contains("Test Template") &&
            a.OldValue.Contains("Test Description") &&
            a.OldValue.Contains("Test Category") &&
            a.OldValue.Contains("\"FieldsCount\":2") &&
            a.OldValue.Contains("\"ApprovalStepsCount\":1")
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_LogsInformation()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var deletedBy = Guid.NewGuid();
        var command = new DeleteRequestTemplateCommand
        {
            Id = templateId,
            DeletedBy = deletedBy
        };

        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "Test Template",
            Description = "Test Description",
            Category = "Test Category",
            Fields = new List<RequestTemplateField>(),
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>()
        };

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        _unitOfWorkMock.Setup(x => x.RequestRepository.GetByTemplateIdAsync(templateId))
            .ReturnsAsync(new List<Request>());

        _unitOfWorkMock.Setup(x => x.RequestTemplateRepository.DeleteAsync(templateId))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.AuditLogRepository.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(Guid.NewGuid());

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting request template")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request template deleted successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

