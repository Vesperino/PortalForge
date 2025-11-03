using PortalForge.Domain.Entities;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Repository interface for managing request comments.
/// </summary>
public interface IRequestCommentRepository
{
    Task<RequestComment?> GetByIdAsync(Guid id);
    Task<IEnumerable<RequestComment>> GetByRequestIdAsync(Guid requestId);
    Task<Guid> CreateAsync(RequestComment comment);
    Task UpdateAsync(RequestComment comment);
    Task DeleteAsync(Guid id);
}
