using PortalForge.Domain.Entities;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Repository interface for managing request edit history.
/// Provides audit trail for all request modifications.
/// </summary>
public interface IRequestEditHistoryRepository
{
    Task<RequestEditHistory?> GetByIdAsync(Guid id);
    Task<IEnumerable<RequestEditHistory>> GetByRequestIdAsync(Guid requestId);
    Task<Guid> CreateAsync(RequestEditHistory editHistory);
    Task DeleteAsync(Guid id);
}
