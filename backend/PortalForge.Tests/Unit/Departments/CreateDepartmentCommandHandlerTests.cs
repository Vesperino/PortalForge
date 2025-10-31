using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.Departments.Commands.CreateDepartment;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Departments;

public class CreateDepartmentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUnifiedValidatorService> _mockValidator;
    private readonly Mock<ILogger<CreateDepartmentCommandHandler>> _mockLogger;
    private readonly CreateDepartmentCommandHandler _handler;

    public CreateDepartmentCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockValidator = new Mock<IUnifiedValidatorService>();
        _mockLogger = new Mock<ILogger<CreateDepartmentCommandHandler>>();
        _handler = new CreateDepartmentCommandHandler(
            _mockUnitOfWork.Object,
            _mockValidator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesDepartment()
    {
        // Arrange
        var command = new CreateDepartmentCommand
        {
            Name = "Engineering",
            Description = "Engineering Department",
            ParentDepartmentId = null,
            DepartmentHeadId = null
        };

        var createdDepartmentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.CreateAsync(It.IsAny<Department>()))
            .ReturnsAsync((Department d) => { d.Id = createdDepartmentId; return d; });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(createdDepartmentId, result);
        _mockValidator.Verify(v => v.ValidateAsync(command), Times.Once);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.CreateAsync(
            It.Is<Department>(d =>
                d.Name == "Engineering" &&
                d.Description == "Engineering Department" &&
                d.ParentDepartmentId == null &&
                d.HeadOfDepartmentId == null &&
                d.IsActive == true
            )), Times.Once);
    }

    [Fact]
    public async Task Handle_WithParentDepartment_CreatesDepartmentWithParent()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        var command = new CreateDepartmentCommand
        {
            Name = "Backend Team",
            Description = "Backend Development Team",
            ParentDepartmentId = parentId,
            DepartmentHeadId = null
        };

        var createdDepartmentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.CreateAsync(It.IsAny<Department>()))
            .ReturnsAsync((Department d) => { d.Id = createdDepartmentId; return d; });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(createdDepartmentId, result);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.CreateAsync(
            It.Is<Department>(d =>
                d.Name == "Backend Team" &&
                d.ParentDepartmentId == parentId
            )), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDepartmentHead_CreatesDepartmentWithHead()
    {
        // Arrange
        var headId = Guid.NewGuid();

        var command = new CreateDepartmentCommand
        {
            Name = "Marketing",
            DepartmentHeadId = headId
        };

        var createdDepartmentId = Guid.NewGuid();

        _mockUnitOfWork.Setup(u => u.DepartmentRepository.CreateAsync(It.IsAny<Department>()))
            .ReturnsAsync((Department d) => { d.Id = createdDepartmentId; return d; });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(createdDepartmentId, result);
        _mockUnitOfWork.Verify(u => u.DepartmentRepository.CreateAsync(
            It.Is<Department>(d =>
                d.Name == "Marketing" &&
                d.HeadOfDepartmentId == headId
            )), Times.Once);
    }
}
