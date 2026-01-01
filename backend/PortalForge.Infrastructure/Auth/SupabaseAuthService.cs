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

public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly ISupabaseClientFactory _clientFactory;
    private readonly Lazy<Task<Client>> _lazyClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly ILogger<SupabaseAuthService> _logger;
    private readonly EmailVerificationTracker _verificationTracker;
    private readonly string _frontendUrl;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    private readonly string _supabaseServiceRoleKey;

    public SupabaseAuthService(
        IOptions<SupabaseSettings> supabaseSettings,
        IOptions<AppSettings> appSettings,
        ISupabaseClientFactory clientFactory,
        ApplicationDbContext dbContext,
        IEmailService emailService,
        EmailVerificationTracker verificationTracker,
        ILogger<SupabaseAuthService> logger)
    {
        var settings = supabaseSettings.Value;
        _clientFactory = clientFactory;
        _dbContext = dbContext;
        _emailService = emailService;
        _verificationTracker = verificationTracker;
        _logger = logger;
        _frontendUrl = appSettings.Value.FrontendUrl;
        _supabaseUrl = settings.Url;
        _supabaseKey = settings.Key;
        _supabaseServiceRoleKey = settings.ServiceRoleKey;

        _lazyClient = new Lazy<Task<Client>>(() => _clientFactory.CreateClientAsync());
    }

    private Task<Client> GetClientAsync() => _lazyClient.Value;

    public async Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        try
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", email);

            // Check if user already exists in our database
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                // If user exists and email is verified, reject registration
                if (existingUser.IsEmailVerified)
                {
                    _logger.LogWarning("Verified user with email {Email} already exists", email);
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "Email already exists. Please choose a different email"
                    };
                }

                // For unverified users, just return success so frontend redirects to verify-email page
                // User can use "Resend Email" button on verify-email page
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

            // Register with Supabase Auth with redirect URL
            var redirectUrl = $"{_frontendUrl}/auth/callback";
            var signUpOptions = new SignUpOptions
            {
                RedirectTo = redirectUrl
            };

            _logger.LogInformation("Registering user with redirect URL: {RedirectUrl}", redirectUrl);
            var client = await GetClientAsync();
            var signUpResponse = await client.Auth.SignUp(email, password, signUpOptions);

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

            // Note: Supabase automatically sends verification email
            // Make sure to configure Site URL and Redirect URLs in Supabase Dashboard
            _logger.LogInformation("User registered successfully: {Email}. Supabase will send verification email.", email);

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
                ErrorMessage = "Registration failed. Please try again later."
            };
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
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password"
                };
            }

            // Update last login time and get email verification status
            var userId = Guid.Parse(signInResponse.User!.Id);
            var user = await _dbContext.Users.FindAsync(userId);

            bool isEmailVerified = false;

            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                isEmailVerified = user.IsEmailVerified;
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("User logged in successfully: {Email}, EmailVerified: {IsEmailVerified}", email, isEmailVerified);

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
                ExpiresAt = expiresAt,
                RequiresEmailVerification = !isEmailVerified
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", email);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Login failed. Please check your credentials and try again."
            };
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
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password"
                };
            }

            var userId = Guid.Parse(signInResponse.User!.Id);

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
            _logger.LogError(ex, "Error during sign in for email: {Email}", email);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Sign in failed. Please check your credentials and try again."
            };
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

            // Set the session with the refresh token, then refresh it
            // Note: We need to provide both access token and refresh token to SetSession
            // Since we only have refresh token, we use RefreshSession directly
            // First, we need to set the session using the Supabase REST API
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);

            var requestBody = new
            {
                refresh_token = refreshToken
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(
                $"{_supabaseUrl}/auth/v1/token?grant_type=refresh_token",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Token refresh failed: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Token refresh failed"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(responseContent);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Session refresh failed. Please log in again."
            };
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email)
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
                ErrorMessage = "Password reset failed. Please try again later."
            };
        }
    }

    public async Task<bool> UpdatePasswordAsync(string accessToken, string refreshToken, string newPassword)
    {
        try
        {
            _logger.LogInformation("Attempting password update");

            // Use the Supabase REST API directly to update the password
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var requestBody = new
            {
                password = newPassword
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PutAsync(
                $"{_supabaseUrl}/auth/v1/user",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Password update failed: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return false;
            }

            _logger.LogInformation("Password updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password update");
            return false;
        }
    }

    public async Task<bool> AdminUpdatePasswordAsync(Guid userId, string newPassword)
    {
        try
        {
            _logger.LogInformation("Admin attempting to update password for user: {UserId}", userId);

            // Use Supabase Admin API to update user password
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", _supabaseServiceRoleKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseServiceRoleKey}");

            var requestBody = new
            {
                password = newPassword
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PutAsync(
                $"{_supabaseUrl}/auth/v1/admin/users/{userId}",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Admin password update failed for user {UserId}: {StatusCode} - {Error}",
                    userId, response.StatusCode, errorContent);
                return false;
            }

            _logger.LogInformation("Password updated successfully by admin for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin password update for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> VerifyEmailAsync(string accessToken)
    {
        try
        {
            _logger.LogInformation("Attempting email verification with access token");

            // Get user from access token
            var userId = await GetUserIdFromTokenAsync(accessToken);

            if (userId == null)
            {
                _logger.LogWarning("Email verification failed - invalid token");
                return false;
            }

            // Update user in our database
            var dbUser = await _dbContext.Users.FindAsync(userId);
            if (dbUser != null)
            {
                dbUser.IsEmailVerified = true;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Email verified successfully for user: {UserId}", dbUser.Id);
                return true;
            }
            else
            {
                _logger.LogWarning("User not found in database: {UserId}", userId);
                return false;
            }
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
            _logger.LogInformation("Attempting to get user ID from access token");

            // Get user from Supabase using the access token
            var client = await GetClientAsync();
            var user = await client.Auth.GetUser(accessToken);

            if (user == null)
            {
                _logger.LogWarning("No user found for provided access token");
                return null;
            }

            _logger.LogInformation("Successfully extracted user ID: {UserId}", user.Id);
            return Guid.Parse(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting user ID from token");
            return null;
        }
    }

    public async Task<bool> ResendVerificationEmailAsync(string email)
    {
        try
        {
            _logger.LogInformation("Resending verification email to: {Email}", email);

            // Check rate limiting (2 minute cooldown)
            if (!_verificationTracker.CanResendEmail(email))
            {
                var remaining = _verificationTracker.GetRemainingCooldown(email);
                _logger.LogWarning("Rate limit exceeded for {Email}. {Seconds} seconds remaining",
                    email, remaining.TotalSeconds);
                return false;
            }

            // Check if user exists and is not verified
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
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

            // Record this resend attempt
            _verificationTracker.RecordResend(email);

            // Resend verification email via Supabase REST API
            var redirectUrl = $"{_frontendUrl}/auth/callback";

            using var httpClient = new HttpClient();

            var requestBody = new
            {
                type = "signup",
                email = email,
                options = new
                {
                    redirect_to = redirectUrl
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_supabaseUrl}/auth/v1/resend")
            {
                Content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Add("apikey", _supabaseKey);

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to resend verification email to {Email}. Status: {Status}, Error: {Error}",
                    email, response.StatusCode, errorContent);
                return false;
            }

            _logger.LogInformation("Successfully resent verification email to {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification email to: {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Admin-only registration that creates users without sending confirmation emails.
    /// Uses Supabase Admin API to bypass email rate limits and auto-verify users.
    /// </summary>
    public async Task<AuthResult> AdminRegisterAsync(string email, string password, string firstName, string lastName)
    {
        try
        {
            _logger.LogInformation("Admin creating user with email: {Email}", email);

            // Check if user already exists in our database
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                _logger.LogWarning("User with email {Email} already exists", email);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Email already exists. Please choose a different email"
                };
            }

            // Create user via Supabase Admin API (no confirmation email sent)
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", _supabaseServiceRoleKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseServiceRoleKey}");

            var requestBody = new
            {
                email = email,
                password = password,
                email_confirm = true, // Auto-confirm email
                user_metadata = new
                {
                    first_name = firstName,
                    last_name = lastName
                }
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(
                $"{_supabaseUrl}/auth/v1/admin/users",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create user via Admin API: {Error}", errorContent);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Registration error: {errorContent}"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var userResponse = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(responseContent);
            var userId = userResponse.GetProperty("id").GetString();

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("Failed to get user ID from Admin API response");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Registration failed - invalid response"
                };
            }

            // Create user in our database (auto-verified)
            var user = new DomainUser
            {
                Id = Guid.Parse(userId),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = true, // Auto-verified for admin-created users
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin registration for email: {Email}", email);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Registration failed. Please try again later."
            };
        }
    }
}
