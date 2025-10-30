using PortalForge.Application.Interfaces;

namespace PortalForge.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserRepository { get; }
    INewsRepository NewsRepository { get; }
    IEventRepository EventRepository { get; }
    IPermissionRepository PermissionRepository { get; }
    IRoleGroupRepository RoleGroupRepository { get; }
    IRoleGroupPermissionRepository RoleGroupPermissionRepository { get; }
    IUserRoleGroupRepository UserRoleGroupRepository { get; }
    IAuditLogRepository AuditLogRepository { get; }
    IHashtagRepository HashtagRepository { get; }
    IRequestTemplateRepository RequestTemplateRepository { get; }
    IRequestRepository RequestRepository { get; }
    INotificationRepository NotificationRepository { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
