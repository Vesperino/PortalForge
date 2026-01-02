using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetFirstByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.Role == role)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        string? department,
        string? position,
        string? role,
        bool? isActive,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(searchLower) ||
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(u => u.Department == department);
        }

        if (!string.IsNullOrWhiteSpace(position))
        {
            query = query.Where(u => u.Position == position);
        }

        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, ignoreCase: true, out var userRole))
        {
            query = query.Where(u => u.Role == userRole);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDescending);

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }

    private static IQueryable<User> ApplySorting(IQueryable<User> query, string? sortBy, bool sortDescending)
    {
        Expression<Func<User, object>> keySelector = sortBy?.ToLower() switch
        {
            "email" => u => u.Email,
            "firstname" => u => u.FirstName,
            "lastname" => u => u.LastName,
            "department" => u => u.Department,
            "position" => u => u.Position,
            "createdat" => u => u.CreatedAt,
            _ => u => u.CreatedAt
        };

        return sortDescending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }

    public async Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        return user.Id;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }

    public async Task<List<User>> SearchAsync(
        string query,
        bool onlyActive,
        Guid? departmentId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var searchLower = query.ToLower();

        var dbQuery = _context.Users
            .AsNoTracking()
            .Where(u => !onlyActive || u.IsActive)
            .Where(u =>
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower) ||
                (u.Department != null && u.Department.ToLower().Contains(searchLower)) ||
                (u.Position != null && u.Position.ToLower().Contains(searchLower)));

        if (departmentId.HasValue)
        {
            dbQuery = dbQuery.Where(u => u.DepartmentId == departmentId.Value);
        }

        return await dbQuery
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
