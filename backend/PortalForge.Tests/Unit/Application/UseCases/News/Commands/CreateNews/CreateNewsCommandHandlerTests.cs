using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.News.Commands.CreateNews;
using PortalForge.Domain.Entities;
using Xunit;

namespace PortalForge.Tests.Unit.Application.UseCases.News.Commands.CreateNews;

public class CreateNewsCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUnifiedValidatorService> _validatorMock;
    private readonly Mock<ILogger<CreateNewsCommandHandler>> _loggerMock;
    private readonly CreateNewsCommandHandler _handler;

    public CreateNewsCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IUnifiedValidatorService>();
        _loggerMock = new Mock<ILogger<CreateNewsCommandHandler>>();

        _handler = new CreateNewsCommandHandler(
            _unitOfWorkMock.Object,
            _validatorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesNews()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new CreateNewsCommand
        {
            Title = "Test News",
            Content = "Test Content",
            Excerpt = "Test Excerpt",
            AuthorId = authorId,
            Category = NewsCategory.Announcement
        };

        var author = new User { Id = authorId, Email = "test@example.com" };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(authorId))
            .ReturnsAsync(author);

        _unitOfWorkMock.Setup(x => x.NewsRepository.CreateAsync(It.IsAny<Domain.Entities.News>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(1);
        _unitOfWorkMock.Verify(x => x.NewsRepository.CreateAsync(It.Is<Domain.Entities.News>(n =>
            n.Title == "Test News" &&
            n.Content == "Test Content" &&
            n.Excerpt == "Test Excerpt" &&
            n.AuthorId == authorId
        )), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_AuthorNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new CreateNewsCommand
        {
            Title = "Test News",
            Content = "Test Content",
            Excerpt = "Test Excerpt",
            AuthorId = Guid.NewGuid(),
            Category = NewsCategory.Announcement
        };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Author*not found*");
    }

    [Fact]
    public async Task Handle_EventIdProvided_ValidatesEventExists()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var eventId = 1;
        var command = new CreateNewsCommand
        {
            Title = "Test News",
            Content = "Test Content",
            Excerpt = "Test Excerpt",
            AuthorId = authorId,
            Category = NewsCategory.Event,
            EventId = eventId
        };

        var author = new User { Id = authorId, Email = "test@example.com" };
        var eventEntity = new Event { Id = eventId, Title = "Test Event" };

        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(authorId))
            .ReturnsAsync(author);
        _unitOfWorkMock.Setup(x => x.EventRepository.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);
        _unitOfWorkMock.Setup(x => x.NewsRepository.CreateAsync(It.IsAny<Domain.Entities.News>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(1);
        _unitOfWorkMock.Verify(x => x.EventRepository.GetByIdAsync(eventId), Times.Once);
    }
}
