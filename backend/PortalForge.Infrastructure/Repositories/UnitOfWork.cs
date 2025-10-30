using Microsoft.EntityFrameworkCore.Storage;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private IUserRepository? _userRepository;
    private INewsRepository? _newsRepository;
    private IEventRepository? _eventRepository;
    private IPermissionRepository? _permissionRepository;
    private IRoleGroupRepository? _roleGroupRepository;
    private IRoleGroupPermissionRepository? _roleGroupPermissionRepository;
    private IUserRoleGroupRepository? _userRoleGroupRepository;
    private IAuditLogRepository? _auditLogRepository;
    private IHashtagRepository? _hashtagRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository UserRepository =>
        _userRepository ??= new UserRepository(_context);

    public INewsRepository NewsRepository =>
        _newsRepository ??= new NewsRepository(_context);

    public IEventRepository EventRepository =>
        _eventRepository ??= new EventRepository(_context);

    public IPermissionRepository PermissionRepository =>
        _permissionRepository ??= new PermissionRepository(_context);

    public IRoleGroupRepository RoleGroupRepository =>
        _roleGroupRepository ??= new RoleGroupRepository(_context);

    public IRoleGroupPermissionRepository RoleGroupPermissionRepository =>
        _roleGroupPermissionRepository ??= new RoleGroupPermissionRepository(_context);

    public IUserRoleGroupRepository UserRoleGroupRepository =>
        _userRoleGroupRepository ??= new UserRoleGroupRepository(_context);

    public IAuditLogRepository AuditLogRepository =>
        _auditLogRepository ??= new AuditLogRepository(_context);

    public IHashtagRepository HashtagRepository =>
        _hashtagRepository ??= new HashtagRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
