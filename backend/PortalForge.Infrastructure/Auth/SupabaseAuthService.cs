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
    private readonly Client _supabaseClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly ILogger<SupabaseAuthService> _logger;
    private readonly EmailVerificationTracker _verificationTracker;
    private readonly string _frontendUrl;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;

    public SupabaseAuthService(
        IOptions<SupabaseSettings> supabaseSettings,
        IOptions<AppSettings> appSettings,
        ApplicationDbContext dbContext,
        IEmailService emailService,
        EmailVerificationTracker verificationTracker,
        ILogger<SupabaseAuthService> logger)
    {
        var settings = supabaseSettings.Value;
        _dbContext = dbContext;
        _emailService = emailService;
        _verificationTracker = verificationTracker;
        _logger = logger;
        _frontendUrl = appSettings.Value.FrontendUrl;
        _supabaseUrl = settings.Url;
        _supabaseKey = settings.Key;

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
            var signUpResponse = await _supabaseClient.Auth.SignUp(email, password, signUpOptions);

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
                ErrorMessage = $"Login error: {ex.Message}"
            };
        }
    }

    public async Task<AuthResult> SignInAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Attempting sign in for email: {Email}", email);

            var signInResponse = await _supabaseClient.Auth.SignIn(email, password);

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
                ErrorMessage = $"Sign in error: {ex.Message}"
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
            var user = await _supabaseClient.Auth.GetUser(accessToken);

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
}
