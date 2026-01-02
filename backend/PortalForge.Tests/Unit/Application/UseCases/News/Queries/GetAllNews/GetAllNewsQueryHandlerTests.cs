using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.News.Queries.GetAllNews;
using PortalForge.Domain.Entities;
using Xunit;
using NewsEntity = PortalForge.Domain.Entities.News;

namespace PortalForge.Tests.Unit.Application.UseCases.News.Queries.GetAllNews;

public class GetAllNewsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<GetAllNewsQueryHandler>> _loggerMock;
    private readonly GetAllNewsQueryHandler _handler;

    public GetAllNewsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<GetAllNewsQueryHandler>>();
        _handler = new GetAllNewsQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    private void SetupPaginatedNewsReturn(IEnumerable<NewsEntity> items, int totalCount)
    {
        _unitOfWorkMock.Setup(x => x.NewsRepository.GetPaginatedAsync(
            It.IsAny<string?>(),
            It.IsAny<int?>(),
            It.IsAny<bool?>(),
            It.IsAny<List<string>?>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, totalCount));
    }

    [Fact]
    public async Task Handle_DefaultQuery_CallsRepositoryWithDefaultPagination()
    {
        // Arrange
        var query = new GetAllNewsQuery();
        SetupPaginatedNewsReturn(new List<NewsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.NewsRepository.GetPaginatedAsync(
            null, null, null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_PassesCategoryToRepository()
    {
        // Arrange
        var query = new GetAllNewsQuery { Category = "Announcement" };
        SetupPaginatedNewsReturn(new List<NewsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.NewsRepository.GetPaginatedAsync(
            "Announcement", null, null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDepartmentFilter_PassesDepartmentIdToRepository()
    {
        // Arrange
        var departmentId = 5;
        var query = new GetAllNewsQuery { DepartmentId = departmentId };
        SetupPaginatedNewsReturn(new List<NewsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.NewsRepository.GetPaginatedAsync(
            null, departmentId, null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithIsEventFilter_PassesIsEventToRepository()
    {
        // Arrange
        var query = new GetAllNewsQuery { IsEvent = true };
        SetupPaginatedNewsReturn(new List<NewsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.NewsRepository.GetPaginatedAsync(
            null, null, true, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithHashtagsFilter_ParsesAndPassesHashtags()
    {
        // Arrange
        var query = new GetAllNewsQuery { Hashtags = "tech,news,update" };
        SetupPaginatedNewsReturn(new List<NewsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.NewsRepository.GetPaginatedAsync(
            null, null, null,
            It.Is<List<string>>(h => h.Count == 3 && h.Contains("tech") && h.Contains("news") && h.Contains("update")),
            1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithHashtagsWithSpaces_TrimsHashtags()
    {
        // Arrange
        var query = new GetAllNewsQuery { Hashtags = " tech , news , update " };
        SetupPaginatedNewsReturn(new List<NewsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.NewsRepository.GetPaginatedAsync(
            null, null, null,
            It.Is<List<string>>(h => h[0] == "tech" && h[1] == "news" && h[2] == "update"),
            1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCustomPagination_PassesPageNumberAndSize()
    {
        // Arrange
        var query = new GetAllNewsQuery { PageNumber = 3, PageSize = 25 };
        SetupPaginatedNewsReturn(new List<NewsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.NewsRepository.GetPaginatedAsync(
            null, null, null, null, 3, 25, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPaginatedResponse()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var author = new User { Id = authorId, FirstName = "John", LastName = "Doe" };
        var newsItems = new List<NewsEntity>
        {
            new NewsEntity
            {
                Id = 1,
                Title = "Test News",
                Content = "Test Content",
                Excerpt = "Test Excerpt",
                ImageUrl = "https://example.com/image.jpg",
                AuthorId = authorId,
                Author = author,
                CreatedAt = new DateTime(2024, 1, 15),
                UpdatedAt = new DateTime(2024, 1, 16),
                Views = 100,
                Category = NewsCategory.Announcement,
                IsEvent = false,
                Hashtags = new List<Hashtag> { new Hashtag { Name = "test" } }
            }
        };
        var totalCount = 50;

        var query = new GetAllNewsQuery { PageNumber = 2, PageSize = 10 };
        SetupPaginatedNewsReturn(newsItems, totalCount);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(50);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_MapsNewsItemsCorrectly()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var author = new User { Id = authorId, FirstName = "John", LastName = "Doe" };
        var eventDateTime = new DateTime(2024, 6, 15, 10, 0, 0);
        var newsItems = new List<NewsEntity>
        {
            new NewsEntity
            {
                Id = 1,
                Title = "Event News",
                Content = "Event Content",
                Excerpt = "Event Excerpt",
                ImageUrl = "https://example.com/event.jpg",
                AuthorId = authorId,
                Author = author,
                CreatedAt = new DateTime(2024, 1, 15),
                UpdatedAt = new DateTime(2024, 1, 16),
                Views = 250,
                Category = NewsCategory.Event,
                EventId = 42,
                IsEvent = true,
                EventHashtag = "#summerevent",
                EventDateTime = eventDateTime,
                EventLocation = "Conference Room A",
                EventLatitude = 52.237049m,
                EventLongitude = 21.017532m,
                DepartmentId = 5,
                Hashtags = new List<Hashtag>
                {
                    new Hashtag { Name = "event" },
                    new Hashtag { Name = "summer" }
                }
            }
        };

        var query = new GetAllNewsQuery();
        SetupPaginatedNewsReturn(newsItems, 1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var newsDto = result.Items.First();
        newsDto.Id.Should().Be(1);
        newsDto.Title.Should().Be("Event News");
        newsDto.Content.Should().Be("Event Content");
        newsDto.Excerpt.Should().Be("Event Excerpt");
        newsDto.ImageUrl.Should().Be("https://example.com/event.jpg");
        newsDto.AuthorId.Should().Be(authorId);
        newsDto.AuthorName.Should().Be("John Doe");
        newsDto.CreatedAt.Should().Be(new DateTime(2024, 1, 15));
        newsDto.UpdatedAt.Should().Be(new DateTime(2024, 1, 16));
        newsDto.Views.Should().Be(250);
        newsDto.Category.Should().Be("Event");
        newsDto.EventId.Should().Be(42);
        newsDto.IsEvent.Should().BeTrue();
        newsDto.EventHashtag.Should().Be("#summerevent");
        newsDto.EventDateTime.Should().Be(eventDateTime);
        newsDto.EventLocation.Should().Be("Conference Room A");
        newsDto.EventLatitude.Should().Be(52.237049m);
        newsDto.EventLongitude.Should().Be(21.017532m);
        newsDto.DepartmentId.Should().Be(5);
        newsDto.Hashtags.Should().HaveCount(2);
        newsDto.Hashtags.Should().Contain("event");
        newsDto.Hashtags.Should().Contain("summer");
    }

    [Fact]
    public async Task Handle_NewsWithNullAuthor_SetsAuthorNameToUnknown()
    {
        // Arrange
        var newsItems = new List<NewsEntity>
        {
            new NewsEntity
            {
                Id = 1,
                Title = "Test News",
                Content = "Content",
                Excerpt = "Excerpt",
                AuthorId = Guid.NewGuid(),
                Author = null,
                CreatedAt = DateTime.UtcNow,
                Category = NewsCategory.Announcement,
                Hashtags = new List<Hashtag>()
            }
        };

        var query = new GetAllNewsQuery();
        SetupPaginatedNewsReturn(newsItems, 1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.First().AuthorName.Should().Be("Unknown");
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyListWithCorrectMetadata()
    {
        // Arrange
        var query = new GetAllNewsQuery { PageNumber = 1, PageSize = 10 };
        SetupPaginatedNewsReturn(new List<NewsEntity>(), 0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithAllFilters_PassesAllFiltersToRepository()
    {
        // Arrange
        var query = new GetAllNewsQuery
        {
            Category = "Event",
            DepartmentId = 5,
            IsEvent = true,
            Hashtags = "summer,party",
            PageNumber = 2,
            PageSize = 20
        };
        SetupPaginatedNewsReturn(new List<NewsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.NewsRepository.GetPaginatedAsync(
            "Event", 5, true, It.Is<List<string>>(h => h.Count == 2), 2, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CancellationRequested_PassesCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var query = new GetAllNewsQuery();
        SetupPaginatedNewsReturn(new List<NewsEntity>(), 0);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.NewsRepository.GetPaginatedAsync(
            It.IsAny<string?>(),
            It.IsAny<int?>(),
            It.IsAny<bool?>(),
            It.IsAny<List<string>?>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyHashtagsString_PassesNullToRepository()
    {
        // Arrange
        var query = new GetAllNewsQuery { Hashtags = "" };
        SetupPaginatedNewsReturn(new List<NewsEntity>(), 0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.NewsRepository.GetPaginatedAsync(
            null, null, null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1, 10, 100, 10)]
    [InlineData(1, 10, 5, 1)]
    [InlineData(2, 25, 100, 4)]
    public async Task Handle_CalculatesPaginationMetadataCorrectly(int pageNumber, int pageSize, int totalCount, int expectedTotalPages)
    {
        // Arrange
        var query = new GetAllNewsQuery { PageNumber = pageNumber, PageSize = pageSize };
        SetupPaginatedNewsReturn(new List<NewsEntity>(), totalCount);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(totalCount);
        result.TotalPages.Should().Be(expectedTotalPages);
    }
}
