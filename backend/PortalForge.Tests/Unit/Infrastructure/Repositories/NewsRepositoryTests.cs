using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;
using PortalForge.Infrastructure.Repositories;
using Xunit;

namespace PortalForge.Tests.Unit.Infrastructure.Repositories;

public class NewsRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly NewsRepository _repository;

    public NewsRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new NewsRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetPaginatedAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            await _context.News.AddAsync(CreateNews($"News {i}", i % 2 == 0));
        }
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPaginatedAsync(
            category: null,
            departmentId: null,
            isEvent: null,
            hashtags: null,
            pageNumber: 2,
            pageSize: 10);

        // Assert
        totalCount.Should().Be(25);
        items.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithCategoryFilter_FiltersCorrectly()
    {
        // Arrange
        var announcement = CreateNews("Announcement", isEvent: false);
        announcement.Category = NewsCategory.Announcement;

        var update = CreateNews("Update", isEvent: false);
        update.Category = NewsCategory.Product;

        await _context.News.AddRangeAsync(announcement, update);
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPaginatedAsync(
            category: "Announcement",
            departmentId: null,
            isEvent: null,
            hashtags: null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items.First().Category.Should().Be(NewsCategory.Announcement);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithEventFilter_FiltersCorrectly()
    {
        // Arrange
        var news = CreateNews("Regular News", isEvent: false);
        var eventNews = CreateNews("Event News", isEvent: true);

        await _context.News.AddRangeAsync(news, eventNews);
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPaginatedAsync(
            category: null,
            departmentId: null,
            isEvent: true,
            hashtags: null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items.First().IsEvent.Should().BeTrue();
    }

    [Fact]
    public async Task GetPaginatedAsync_WithDepartmentFilter_FiltersCorrectly()
    {
        // Arrange
        var globalNews = CreateNews("Global News", isEvent: false);
        globalNews.DepartmentId = null;

        var deptNews = CreateNews("Department News", isEvent: false);
        deptNews.DepartmentId = 5;

        var otherDeptNews = CreateNews("Other Dept News", isEvent: false);
        otherDeptNews.DepartmentId = 10;

        await _context.News.AddRangeAsync(globalNews, deptNews, otherDeptNews);
        await _context.SaveChangesAsync();

        // Act - Department 5 should see global news + department 5 news
        var (items, totalCount) = await _repository.GetPaginatedAsync(
            category: null,
            departmentId: 5,
            isEvent: null,
            hashtags: null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        totalCount.Should().Be(2);
        items.Should().Contain(n => n.DepartmentId == null);
        items.Should().Contain(n => n.DepartmentId == 5);
        items.Should().NotContain(n => n.DepartmentId == 10);
    }

    [Fact]
    public async Task GetPaginatedAsync_ReturnsOrderedByCreatedAtDesc()
    {
        // Arrange
        var oldNews = CreateNews("Old News", isEvent: false);
        oldNews.CreatedAt = DateTime.UtcNow.AddDays(-10);

        var newNews = CreateNews("New News", isEvent: false);
        newNews.CreatedAt = DateTime.UtcNow;

        await _context.News.AddRangeAsync(oldNews, newNews);
        await _context.SaveChangesAsync();

        // Act
        var (items, _) = await _repository.GetPaginatedAsync(
            category: null,
            departmentId: null,
            isEvent: null,
            hashtags: null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        items.First().Title.Should().Be("New News");
        items.Last().Title.Should().Be("Old News");
    }

    [Fact]
    public async Task GetPaginatedAsync_WithLastPage_ReturnsRemainingItems()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            await _context.News.AddAsync(CreateNews($"News {i}", false));
        }
        await _context.SaveChangesAsync();

        // Act - Page 3 with size 10 should have 5 items
        var (items, totalCount) = await _repository.GetPaginatedAsync(
            category: null,
            departmentId: null,
            isEvent: null,
            hashtags: null,
            pageNumber: 3,
            pageSize: 10);

        // Assert
        totalCount.Should().Be(25);
        items.Should().HaveCount(5);
    }

    private static News CreateNews(string title, bool isEvent)
    {
        return new News
        {
            Title = title,
            Content = $"Content for {title}",
            Excerpt = $"Excerpt for {title}",
            IsEvent = isEvent,
            Category = NewsCategory.Announcement,
            CreatedAt = DateTime.UtcNow,
            Views = 0,
            Hashtags = new List<Hashtag>()
        };
    }
}
