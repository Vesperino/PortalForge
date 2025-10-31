using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Hashtag entity operations.
/// </summary>
public interface IHashtagRepository
{
    Task<Hashtag?> GetByNormalizedNameAsync(string normalizedName);
    Task<List<Hashtag>> GetOrCreateHashtagsAsync(List<string> hashtagNames);
}




