using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Common.Models;
using Supabase;
using Client = Supabase.Client;

namespace PortalForge.Infrastructure.Auth;

public class SupabaseTokenService : ISupabaseTokenService
{
    private readonly ISupabaseClientFactory _clientFactory;
    private readonly Lazy<Task<Client>> _lazyClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SupabaseTokenService> _logger;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;

    public SupabaseTokenService(
        IOptions<SupabaseSettings> supabaseSettings,
        ISupabaseClientFactory clientFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<SupabaseTokenService> logger)
    {
        var settings = supabaseSettings.Value;
        _clientFactory = clientFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _supabaseUrl = settings.Url;
        _supabaseKey = settings.Key;

        _lazyClient = new Lazy<Task<Client>>(() => _clientFactory.CreateClientAsync());
    }

    private Task<Client> GetClientAsync() => _lazyClient.Value;

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting token refresh with provided refresh token");

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("Refresh token is null or empty");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Refresh token is required"
                };
            }

            var httpClient = _httpClientFactory.CreateClient("Supabase");
            httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);

            var requestBody = new { refresh_token = refreshToken };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(
                $"{_supabaseUrl}/auth/v1/token?grant_type=refresh_token",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Token refresh failed: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Token refresh failed"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var accessToken = tokenResponse.GetProperty("access_token").GetString();
            var newRefreshToken = tokenResponse.GetProperty("refresh_token").GetString();
            var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Token refresh failed - no access token returned");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Token refresh failed"
                };
            }

            _logger.LogInformation("Token refreshed successfully");

            return new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken ?? refreshToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn)
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Token refresh was cancelled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during token refresh");
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Unable to connect to authentication service"
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse token refresh response");
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Invalid response from authentication service"
            };
        }
    }

    public async Task<Guid?> GetUserIdFromTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to get user ID from access token");

            var client = await GetClientAsync();
            var user = await client.Auth.GetUser(accessToken);

            if (user == null)
            {
                _logger.LogWarning("No user found for provided access token");
                return null;
            }

            if (!Guid.TryParse(user.Id, out var userId))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", user.Id);
                return null;
            }

            _logger.LogInformation("Successfully extracted user ID: {UserId}", user.Id);
            return userId;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get user ID from token was cancelled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error extracting user ID from token");
            return null;
        }
    }
}
