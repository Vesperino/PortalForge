using PortalForge.Application.Common.Models;

namespace PortalForge.Application.Common.Interfaces;

public interface ISupabaseAuthService
{
    Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> SignInAsync(string email, string password);
    Task<bool> LogoutAsync(string accessToken);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> SendPasswordResetEmailAsync(string email);
    Task<AuthResult> ResetPasswordAsync(string accessToken, string newPassword);
    Task<bool> UpdatePasswordAsync(string accessToken, string refreshToken, string newPassword);
    Task<bool> VerifyEmailAsync(string token);
    Task<Guid?> GetUserIdFromTokenAsync(string accessToken);
    Task<bool> ResendVerificationEmailAsync(string email);
}
