using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class HashtagRepository : IHashtagRepository
{
    private readonly ApplicationDbContext _context;

    public HashtagRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Hashtag?> GetByNormalizedNameAsync(string normalizedName)
    {
        return await _context.Hashtags
            .FirstOrDefaultAsync(h => h.NormalizedName == normalizedName);
    }

    public async Task<List<Hashtag>> GetOrCreateHashtagsAsync(List<string> hashtagNames)
    {
        var hashtags = new List<Hashtag>();

        foreach (var name in hashtagNames)
        {
            // Ensure hashtag starts with #
            var cleanName = name.StartsWith("#") ? name : $"#{name}";
            var normalizedName = cleanName.ToLower();

            // Try to find existing hashtag
            var existing = await GetByNormalizedNameAsync(normalizedName);

            if (existing != null)
            {
                hashtags.Add(existing);
            }
            else
            {
                // Create new hashtag
                var newHashtag = new Hashtag
                {
                    Name = cleanName,
                    NormalizedName = normalizedName,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Hashtags.AddAsync(newHashtag);
                hashtags.Add(newHashtag);
            }
        }

        return hashtags;
    }
}


