using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class NewsRepository : INewsRepository
{
    private readonly ApplicationDbContext _context;

    public NewsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<News?> GetByIdAsync(int id)
    {
        return await _context.News
            .Include(n => n.Author)
            .Include(n => n.Event)
            .Include(n => n.Hashtags)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<News>> GetAllAsync()
    {
        return await _context.News
            .Include(n => n.Author)
            .Include(n => n.Event)
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<News>> GetByCategoryAsync(NewsCategory category)
    {
        return await _context.News
            .Include(n => n.Author)
            .Include(n => n.Event)
            .Where(n => n.Category == category)
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<News>> GetLatestAsync(int count)
    {
        return await _context.News
            .Include(n => n.Author)
            .Include(n => n.Event)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<News>> GetByDepartmentAsync(int departmentId)
    {
        return await _context.News
            .Include(n => n.Author)
            .Include(n => n.Event)
            .Where(n => n.DepartmentId == null || n.DepartmentId == departmentId)
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<News>> GetEventsAsync()
    {
        return await _context.News
            .Include(n => n.Author)
            .Include(n => n.Event)
            .Where(n => n.IsEvent)
            .OrderByDescending(n => n.EventDateTime ?? n.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<News>> GetUpcomingEventsAsync(DateTime fromDate)
    {
        return await _context.News
            .Include(n => n.Author)
            .Include(n => n.Event)
            .Where(n => n.IsEvent && n.EventDateTime >= fromDate)
            .OrderBy(n => n.EventDateTime)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<News>> GetByHashtagsAsync(List<string> hashtags)
    {
        // Normalize hashtags for case-insensitive search
        var normalizedHashtags = hashtags
            .Select(h => h.StartsWith("#") ? h.ToLower() : $"#{h}".ToLower())
            .ToList();

        return await _context.News
            .Include(n => n.Author)
            .Include(n => n.Event)
            .Include(n => n.Hashtags)
            .Where(n => n.Hashtags.Any(h => normalizedHashtags.Contains(h.NormalizedName)))
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> CreateAsync(News news)
    {
        var entry = await _context.News.AddAsync(news);
        // Note: ID will be 0 until SaveChangesAsync() is called
        // The caller must call SaveChangesAsync() and then access news.Id
        return news.Id;
    }

    public async Task UpdateAsync(News news)
    {
        _context.News.Update(news);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var news = await _context.News.FindAsync(id);
        if (news != null)
        {
            _context.News.Remove(news);
        }
    }

    public async Task IncrementViewsAsync(int id)
    {
        var news = await _context.News.FindAsync(id);
        if (news != null)
        {
            news.Views++;
        }
    }
}
