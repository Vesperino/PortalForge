namespace PortalForge.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserRepository { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
