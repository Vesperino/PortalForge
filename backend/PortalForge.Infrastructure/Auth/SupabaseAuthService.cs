using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Common.Models;
using PortalForge.Infrastructure.Persistence;
using Supabase;
using Supabase.Gotrue;
using Client = Supabase.Client;
using DomainUser = PortalForge.Domain.Entities.User;
using SupabaseUser = Supabase.Gotrue.User;

namespace PortalForge.Infrastructure.Auth;

public class SupabaseAuthService : IAuthService
{
    private readonly Client _supabaseClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly ILogger<SupabaseAuthService> _logger;
    private readonly string _frontendUrl;

    public SupabaseAuthService(
        IOptions<SupabaseSettings> supabaseSettings,
        IOptions<AppSettings> appSettings,
        ApplicationDbContext dbContext,
        IEmailService emailService,
        ILogger<SupabaseAuthService> logger)
    {
        var settings = supabaseSettings.Value;
        _dbContext = dbContext;
        _emailService = emailService;
        _logger = logger;
        _frontendUrl = appSettings.Value.FrontendUrl;

        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false
        };

        _supabaseClient = new Client(settings.Url, settings.Key, options);
        _supabaseClient.InitializeAsync().Wait();
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        try
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", email);

            // Check if user already exists in our database
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                _logger.LogWarning("User with email {Email} already exists", email);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "User with this email already exists"
                };
            }

            // Register with Supabase Auth
            var signUpResponse = await _supabaseClient.Auth.SignUp(email, password);

            if (signUpResponse?.User == null)
            {
                _logger.LogError("Supabase registration failed for email: {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Registration failed"
                };
            }

            // Create user in our database
            var user = new DomainUser
            {
                Id = Guid.Parse(signUpResponse.User!.Id),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = false,
                Role = Domain.Entities.UserRole.Employee,
                Department = "Unassigned",
                Position = "Unassigned"
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Send verification email
            var verificationLink = $"{_frontendUrl}/verify-email?token={signUpResponse.User.Id}";
            await _emailService.SendVerificationEmailAsync(email, verificationLink);

            _logger.LogInformation("User registered successfully: {Email}", email);

            DateTime? expiresAt = signUpResponse.ExpiresAt() != default
                ? signUpResponse.ExpiresAt()
                : null;

            return new AuthResult
            {
                Success = true,
                UserId = user.Id,
                Email = user.Email,
                AccessToken = signUpResponse.AccessToken,
                RefreshToken = signUpResponse.RefreshToken,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", email);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Registration error: {ex.Message}"
            };
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Attempting login for email: {Email}", email);

            var signInResponse = await _supabaseClient.Auth.SignIn(email, password);

            if (signInResponse?.User == null)
            {
                _logger.LogWarning("Login failed for email: {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password"
                };
            }

            // Update last login time
            var userId = Guid.Parse(signInResponse.User!.Id);
            var user = await _dbContext.Users.FindAsync(userId);

            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("User logged in successfully: {Email}", email);

            DateTime? expiresAt = signInResponse.ExpiresAt() != default
                ? signInResponse.ExpiresAt()
                : null;

            return new AuthResult
            {
                Success = true,
                UserId = userId,
                Email = email,
                AccessToken = signInResponse.AccessToken,
                RefreshToken = signInResponse.RefreshToken,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", email);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Login error: {ex.Message}"
            };
        }
    }

    public async Task<bool> LogoutAsync(string accessToken)
    {
        try
        {
            _logger.LogInformation("Attempting logout");
            await _supabaseClient.Auth.SignOut();
            _logger.LogInformation("User logged out successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return false;
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            _logger.LogInformation("Attempting token refresh");

            var refreshResponse = await _supabaseClient.Auth.RefreshSession();

            if (refreshResponse?.AccessToken == null)
            {
                _logger.LogWarning("Token refresh failed");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Token refresh failed"
                };
            }

            _logger.LogInformation("Token refreshed successfully");

            DateTime? expiresAt = refreshResponse.ExpiresAt() != default
                ? refreshResponse.ExpiresAt()
                : null;

            return new AuthResult
            {
                Success = true,
                AccessToken = refreshResponse.AccessToken,
                RefreshToken = refreshResponse.RefreshToken,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Token refresh error: {ex.Message}"
            };
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email)
    {
        try
        {
            _logger.LogInformation("Sending password reset email to: {Email}", email);

            await _supabaseClient.Auth.ResetPasswordForEmail(email);

            var resetLink = $"{_frontendUrl}/reset-password?email={email}";
            await _emailService.SendPasswordResetEmailAsync(email, resetLink);

            _logger.LogInformation("Password reset email sent to: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to: {Email}", email);
            return false;
        }
    }

    public async Task<AuthResult> ResetPasswordAsync(string accessToken, string newPassword)
    {
        try
        {
            _logger.LogInformation("Attempting password reset");

            var updateResponse = await _supabaseClient.Auth.Update(new UserAttributes
            {
                Password = newPassword
            });

            if (updateResponse == null)
            {
                _logger.LogWarning("Password reset failed");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Password reset failed"
                };
            }

            var currentUser = _supabaseClient.Auth.CurrentUser;
            if (currentUser?.Email != null)
            {
                // Send confirmation email
                await _emailService.SendPasswordChangedEmailAsync(currentUser.Email);
                _logger.LogInformation("Password reset successfully for user: {UserId}", currentUser.Id);

                return new AuthResult
                {
                    Success = true,
                    UserId = Guid.Parse(currentUser.Id),
                    Email = currentUser.Email
                };
            }

            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Could not retrieve user information"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Password reset error: {ex.Message}"
            };
        }
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        try
        {
            _logger.LogInformation("Attempting email verification");

            // Set the session with the token
            await _supabaseClient.Auth.SetSession(token, token);
            var currentUser = _supabaseClient.Auth.CurrentUser;

            if (currentUser != null)
            {
                var dbUser = await _dbContext.Users.FindAsync(Guid.Parse(currentUser.Id));
                if (dbUser != null)
                {
                    dbUser.IsEmailVerified = true;
                    await _dbContext.SaveChangesAsync();
                }
            }

            _logger.LogInformation("Email verified successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return false;
        }
    }

    public async Task<Guid?> GetUserIdFromTokenAsync(string accessToken)
    {
        try
        {
            // Set the session with the access token
            await _supabaseClient.Auth.SetSession(accessToken, "");
            var user = _supabaseClient.Auth.CurrentUser;

            return user != null ? Guid.Parse(user.Id) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting user ID from token");
            return null;
        }
    }
}
