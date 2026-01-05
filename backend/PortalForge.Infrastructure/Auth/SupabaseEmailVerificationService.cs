using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Auth;

public class SupabaseEmailVerificationService : ISupabaseEmailVerificationService
{
    private readonly ISupabaseTokenService _tokenService;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SupabaseEmailVerificationService> _logger;
    private readonly EmailVerificationTracker _verificationTracker;
    private readonly string _frontendUrl;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;

    public SupabaseEmailVerificationService(
        IOptions<SupabaseSettings> supabaseSettings,
        IOptions<AppSettings> appSettings,
        ISupabaseTokenService tokenService,
        ApplicationDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        EmailVerificationTracker verificationTracker,
        ILogger<SupabaseEmailVerificationService> logger)
    {
        var settings = supabaseSettings.Value;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _verificationTracker = verificationTracker;
        _logger = logger;
        _frontendUrl = appSettings.Value.FrontendUrl;
        _supabaseUrl = settings.Url;
        _supabaseKey = settings.Key;
    }

    public async Task<bool> VerifyEmailAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting email verification with access token");

            var userId = await _tokenService.GetUserIdFromTokenAsync(accessToken, cancellationToken);

            if (userId == null)
            {
                _logger.LogWarning("Email verification failed - invalid token");
                return false;
            }

            var dbUser = await _dbContext.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (dbUser != null)
            {
                dbUser.IsEmailVerified = true;
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Email verified successfully for user: {UserId}", dbUser.Id);
                return true;
            }
            else
            {
                _logger.LogWarning("User not found in database: {UserId}", userId);
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Email verification was cancelled");
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during email verification");
            return false;
        }
    }

    public async Task<bool> ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Resending verification email to: {Email}", email);

            if (!_verificationTracker.CanResendEmail(email))
            {
                var remaining = _verificationTracker.GetRemainingCooldown(email);
                _logger.LogWarning("Rate limit exceeded for {Email}. {Seconds} seconds remaining",
                    email, remaining.TotalSeconds);
                return false;
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User with email {Email} not found", email);
                return false;
            }

            if (user.IsEmailVerified)
            {
                _logger.LogWarning("User {Email} is already verified", email);
                return false;
            }

            _verificationTracker.RecordResend(email);

            var redirectUrl = $"{_frontendUrl}/auth/callback";
            var httpClient = _httpClientFactory.CreateClient("Supabase");

            var requestBody = new
            {
                type = "signup",
                email = email,
                options = new { redirect_to = redirectUrl }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_supabaseUrl}/auth/v1/resend")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Add("apikey", _supabaseKey);

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to resend verification email to {Email}. Status: {Status}, Error: {Error}",
                    email, response.StatusCode, errorContent);
                return false;
            }

            _logger.LogInformation("Successfully resent verification email to {Email}", email);
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Resend verification email was cancelled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error resending verification email to: {Email}", email);
            return false;
        }
    }
}
