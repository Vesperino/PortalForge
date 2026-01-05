using System.Text.Json;
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

namespace PortalForge.Infrastructure.Auth;

public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly ISupabaseClientFactory _clientFactory;
    private readonly Lazy<Task<Client>> _lazyClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SupabaseAuthService> _logger;

    // Delegated services
    private readonly ISupabaseTokenService _tokenService;
    private readonly ISupabasePasswordService _passwordService;
    private readonly ISupabaseEmailVerificationService _emailVerificationService;

    private readonly string _frontendUrl;
    private readonly string _supabaseUrl;
    private readonly string _supabaseServiceRoleKey;

    public SupabaseAuthService(
        IOptions<SupabaseSettings> supabaseSettings,
        IOptions<AppSettings> appSettings,
        ISupabaseClientFactory clientFactory,
        ApplicationDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        ISupabaseTokenService tokenService,
        ISupabasePasswordService passwordService,
        ISupabaseEmailVerificationService emailVerificationService,
        ILogger<SupabaseAuthService> logger)
    {
        var settings = supabaseSettings.Value;
        _clientFactory = clientFactory;
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _emailVerificationService = emailVerificationService;
        _logger = logger;
        _frontendUrl = appSettings.Value.FrontendUrl;
        _supabaseUrl = settings.Url;
        _supabaseServiceRoleKey = settings.ServiceRoleKey;

        _lazyClient = new Lazy<Task<Client>>(() => _clientFactory.CreateClientAsync());
    }

    private Task<Client> GetClientAsync() => _lazyClient.Value;

    public async Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        try
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", email);

            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                if (existingUser.IsEmailVerified)
                {
                    _logger.LogWarning("Verified user with email {Email} already exists", email);
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "Email already exists. Please choose a different email"
                    };
                }

                _logger.LogInformation("User {Email} exists but is not verified. Redirecting to verify-email page.", email);
                return new AuthResult
                {
                    Success = true,
                    UserId = existingUser.Id,
                    Email = existingUser.Email,
                    AccessToken = null,
                    RefreshToken = null,
                    RequiresEmailVerification = true
                };
            }

            var redirectUrl = $"{_frontendUrl}/auth/callback";
            var signUpOptions = new SignUpOptions { RedirectTo = redirectUrl };

            _logger.LogInformation("Registering user with redirect URL: {RedirectUrl}", redirectUrl);
            var client = await GetClientAsync();
            var signUpResponse = await client.Auth.SignUp(email, password, signUpOptions);

            if (signUpResponse?.User == null)
            {
                _logger.LogError("Supabase registration failed for email: {Email}", email);
                return new AuthResult { Success = false, ErrorMessage = "Registration failed" };
            }

            if (signUpResponse.User?.Id is null)
            {
                _logger.LogError("User creation failed - no user ID returned from Supabase for email: {Email}", email);
                return new AuthResult { Success = false, ErrorMessage = "Registration failed - no user ID returned" };
            }

            var user = new DomainUser
            {
                Id = Guid.Parse(signUpResponse.User.Id),
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

            _logger.LogInformation("User registered successfully: {Email}. Supabase will send verification email.", email);

            DateTime? expiresAt = signUpResponse.ExpiresAt() != default ? signUpResponse.ExpiresAt() : null;

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
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during registration for email: {Email}", email);
            return new AuthResult { Success = false, ErrorMessage = "Registration failed due to database error" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during registration for email: {Email}", email);
            return new AuthResult { Success = false, ErrorMessage = "Unable to connect to authentication service" };
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Attempting login for email: {Email}", email);

            var client = await GetClientAsync();
            var signInResponse = await client.Auth.SignIn(email, password);

            if (signInResponse?.User == null)
            {
                _logger.LogWarning("Login failed for email: {Email}", email);
                return new AuthResult { Success = false, ErrorMessage = "Invalid email or password" };
            }

            if (signInResponse.User?.Id is null)
            {
                _logger.LogError("Login succeeded but no user ID returned from Supabase for email: {Email}", email);
                return new AuthResult { Success = false, ErrorMessage = "Login failed - invalid user data" };
            }

            var userId = Guid.Parse(signInResponse.User.Id);
            var user = await _dbContext.Users.FindAsync(userId);

            bool isEmailVerified = false;

            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                isEmailVerified = user.IsEmailVerified;
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("User logged in successfully: {Email}, EmailVerified: {IsEmailVerified}", email, isEmailVerified);

            DateTime? expiresAt = signInResponse.ExpiresAt() != default ? signInResponse.ExpiresAt() : null;

            return new AuthResult
            {
                Success = true,
                UserId = userId,
                Email = email,
                AccessToken = signInResponse.AccessToken,
                RefreshToken = signInResponse.RefreshToken,
                ExpiresAt = expiresAt,
                RequiresEmailVerification = !isEmailVerified
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during login for email: {Email}", email);
            return new AuthResult { Success = false, ErrorMessage = "Unable to connect to authentication service" };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during login for email: {Email}", email);
            return new AuthResult { Success = false, ErrorMessage = "Login failed due to database error" };
        }
    }

    public async Task<AuthResult> SignInAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Attempting sign in for email: {Email}", email);

            var client = await GetClientAsync();
            var signInResponse = await client.Auth.SignIn(email, password);

            if (signInResponse?.User == null)
            {
                _logger.LogWarning("Sign in failed for email: {Email}", email);
                return new AuthResult { Success = false, ErrorMessage = "Invalid email or password" };
            }

            if (signInResponse.User?.Id is null)
            {
                _logger.LogError("Sign in succeeded but no user ID returned from Supabase for email: {Email}", email);
                return new AuthResult { Success = false, ErrorMessage = "Sign in failed - invalid user data" };
            }

            var userId = Guid.Parse(signInResponse.User.Id);
            DateTime? expiresAt = signInResponse.ExpiresAt() != default ? signInResponse.ExpiresAt() : null;

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
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during sign in for email: {Email}", email);
            return new AuthResult { Success = false, ErrorMessage = "Unable to connect to authentication service" };
        }
    }

    public async Task<bool> LogoutAsync(string accessToken)
    {
        try
        {
            _logger.LogInformation("Attempting logout");
            var client = await GetClientAsync();
            await client.Auth.SignOut();
            _logger.LogInformation("User logged out successfully");
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during logout");
            return false;
        }
    }

    public async Task<AuthResult> AdminRegisterAsync(string email, string password, string firstName, string lastName)
    {
        try
        {
            _logger.LogInformation("Admin creating user with email: {Email}", email);

            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                _logger.LogWarning("User with email {Email} already exists", email);
                return new AuthResult { Success = false, ErrorMessage = "Email already exists. Please choose a different email" };
            }

            var httpClient = _httpClientFactory.CreateClient("SupabaseAdmin");
            httpClient.DefaultRequestHeaders.Add("apikey", _supabaseServiceRoleKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseServiceRoleKey}");

            var requestBody = new
            {
                email = email,
                password = password,
                email_confirm = true,
                user_metadata = new { first_name = firstName, last_name = lastName }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync($"{_supabaseUrl}/auth/v1/admin/users", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create user via Admin API: {Error}", errorContent);
                return new AuthResult { Success = false, ErrorMessage = $"Registration error: {errorContent}" };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var userResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var userId = userResponse.GetProperty("id").GetString();

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("Failed to get user ID from Admin API response");
                return new AuthResult { Success = false, ErrorMessage = "Registration failed - invalid response" };
            }

            var user = new DomainUser
            {
                Id = Guid.Parse(userId),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = true,
                Role = Domain.Entities.UserRole.Employee,
                Department = "Unassigned",
                Position = "Unassigned"
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User created successfully via Admin API: {Email} (no confirmation email sent)", email);

            return new AuthResult
            {
                Success = true,
                UserId = user.Id,
                Email = user.Email,
                AccessToken = null,
                RefreshToken = null
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during admin registration for email: {Email}", email);
            return new AuthResult { Success = false, ErrorMessage = "Registration failed due to database error" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during admin registration for email: {Email}", email);
            return new AuthResult { Success = false, ErrorMessage = "Unable to connect to authentication service" };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse admin registration response for email: {Email}", email);
            return new AuthResult { Success = false, ErrorMessage = "Invalid response from authentication service" };
        }
    }

    // Delegated methods - maintain backward compatibility with ISupabaseAuthService

    public Task<AuthResult> RefreshTokenAsync(string refreshToken)
        => _tokenService.RefreshTokenAsync(refreshToken);

    public Task<Guid?> GetUserIdFromTokenAsync(string accessToken)
        => _tokenService.GetUserIdFromTokenAsync(accessToken);

    public Task<bool> SendPasswordResetEmailAsync(string email)
        => _passwordService.SendPasswordResetEmailAsync(email);

    public Task<AuthResult> ResetPasswordAsync(string accessToken, string newPassword)
        => _passwordService.ResetPasswordAsync(accessToken, newPassword);

    public Task<bool> UpdatePasswordAsync(string accessToken, string refreshToken, string newPassword)
        => _passwordService.UpdatePasswordAsync(accessToken, refreshToken, newPassword);

    public Task<bool> AdminUpdatePasswordAsync(Guid userId, string newPassword)
        => _passwordService.AdminUpdatePasswordAsync(userId, newPassword);

    public Task<bool> VerifyEmailAsync(string token)
        => _emailVerificationService.VerifyEmailAsync(token);

    public Task<bool> ResendVerificationEmailAsync(string email)
        => _emailVerificationService.ResendVerificationEmailAsync(email);
}
