using PortalForge.Application.Common.Models;

namespace PortalForge.Application.Common.Interfaces;

public interface ISupabasePasswordService
{
    Task<bool> SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<AuthResult> ResetPasswordAsync(string accessToken, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> UpdatePasswordAsync(string accessToken, string refreshToken, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> AdminUpdatePasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken = default);
}
