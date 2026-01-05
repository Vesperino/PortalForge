namespace PortalForge.Application.Common.Interfaces;

public interface ISupabaseEmailVerificationService
{
    Task<bool> VerifyEmailAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<bool> ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default);
}
