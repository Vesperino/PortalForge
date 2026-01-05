using PortalForge.Application.Common.Models;

namespace PortalForge.Application.Common.Interfaces;

public interface ISupabaseTokenService
{
    Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Guid?> GetUserIdFromTokenAsync(string accessToken, CancellationToken cancellationToken = default);
}
