using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Common.Models;
using Supabase;
using Supabase.Gotrue;
using Client = Supabase.Client;

namespace PortalForge.Infrastructure.Auth;

public class SupabasePasswordService : ISupabasePasswordService
{
    private readonly ISupabaseClientFactory _clientFactory;
    private readonly Lazy<Task<Client>> _lazyClient;
    private readonly IEmailService _emailService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SupabasePasswordService> _logger;
    private readonly string _frontendUrl;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    private readonly string _supabaseServiceRoleKey;

    public SupabasePasswordService(
        IOptions<SupabaseSettings> supabaseSettings,
        IOptions<AppSettings> appSettings,
        ISupabaseClientFactory clientFactory,
        IEmailService emailService,
        IHttpClientFactory httpClientFactory,
        ILogger<SupabasePasswordService> logger)
    {
        var settings = supabaseSettings.Value;
        _clientFactory = clientFactory;
        _emailService = emailService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _frontendUrl = appSettings.Value.FrontendUrl;
        _supabaseUrl = settings.Url;
        _supabaseKey = settings.Key;
        _supabaseServiceRoleKey = settings.ServiceRoleKey;

        _lazyClient = new Lazy<Task<Client>>(() => _clientFactory.CreateClientAsync());
    }

    private Task<Client> GetClientAsync() => _lazyClient.Value;

    public async Task<bool> SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending password reset email to: {Email}", email);

            var client = await GetClientAsync();
            await client.Auth.ResetPasswordForEmail(email);

            var resetLink = $"{_frontendUrl}/reset-password?email={email}";
            await _emailService.SendPasswordResetEmailAsync(email, resetLink);

            _logger.LogInformation("Password reset email sent to: {Email}", email);
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Send password reset email was cancelled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error sending password reset email to: {Email}", email);
            return false;
        }
    }

    public async Task<AuthResult> ResetPasswordAsync(string accessToken, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting password reset");

            var client = await GetClientAsync();
            var updateResponse = await client.Auth.Update(new UserAttributes
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

            var currentUser = client.Auth.CurrentUser;
            if (currentUser?.Email != null && Guid.TryParse(currentUser.Id, out var userId))
            {
                await _emailService.SendPasswordChangedEmailAsync(currentUser.Email);
                _logger.LogInformation("Password reset successfully for user: {UserId}", currentUser.Id);

                return new AuthResult
                {
                    Success = true,
                    UserId = userId,
                    Email = currentUser.Email
                };
            }

            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Could not retrieve user information"
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Password reset was cancelled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during password reset");
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Unable to connect to authentication service"
            };
        }
    }

    public async Task<bool> UpdatePasswordAsync(string accessToken, string refreshToken, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting password update");

            var httpClient = _httpClientFactory.CreateClient("Supabase");
            httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var requestBody = new { password = newPassword };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PutAsync(
                $"{_supabaseUrl}/auth/v1/user",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Password update failed: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return false;
            }

            _logger.LogInformation("Password updated successfully");
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Password update was cancelled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during password update");
            return false;
        }
    }

    public async Task<bool> AdminUpdatePasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Admin attempting to update password for user: {UserId}", userId);

            var httpClient = _httpClientFactory.CreateClient("SupabaseAdmin");
            httpClient.DefaultRequestHeaders.Add("apikey", _supabaseServiceRoleKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseServiceRoleKey}");

            var requestBody = new { password = newPassword };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PutAsync(
                $"{_supabaseUrl}/auth/v1/admin/users/{userId}",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Admin password update failed for user {UserId}: {StatusCode} - {Error}",
                    userId, response.StatusCode, errorContent);
                return false;
            }

            _logger.LogInformation("Password updated successfully by admin for user: {UserId}", userId);
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Admin password update was cancelled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during admin password update for user: {UserId}", userId);
            return false;
        }
    }
}
