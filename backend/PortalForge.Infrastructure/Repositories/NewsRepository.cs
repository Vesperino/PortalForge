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

    public async Task<int> CreateAsync(News news)
    {
        await _context.News.AddAsync(news);
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
