using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Infrastructure.Auth;
using PortalForge.Infrastructure.Email;
using PortalForge.Infrastructure.Email.Models;
using PortalForge.Infrastructure.Persistence;
using PortalForge.Infrastructure.Repositories;
using PortalForge.Infrastructure.Validation;
using System.Linq;
using System.Text;

namespace PortalForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register UnifiedValidatorService
        services.AddScoped<IUnifiedValidatorService, UnifiedValidatorService>();

        // Register Email Settings and Service
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();

        // Register App Settings
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        // Register Supabase Settings and Auth Service
        services.Configure<SupabaseSettings>(configuration.GetSection("Supabase"));
        services.AddScoped<ISupabaseAuthService, SupabaseAuthService>();

        // Register File Storage Service
        services.AddScoped<IFileStorageService, PortalForge.Infrastructure.Storage.LocalFileStorageService>();

        // Register Notification Service
        services.AddScoped<PortalForge.Application.Services.INotificationService, PortalForge.Infrastructure.Services.NotificationService>();

        // Register Unit of Work and Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Email Verification Tracker (Singleton for rate limiting)
        services.AddSingleton<EmailVerificationTracker>();

        // Configure JWT Authentication with Supabase
        ConfigureAuthentication(services, configuration);

        return services;
    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var supabaseUrl = configuration["Supabase:Url"];
        var supabaseJwtSecret = configuration["Supabase:JwtSecret"];

        if (string.IsNullOrEmpty(supabaseJwtSecret))
        {
            throw new InvalidOperationException(
                "Supabase:JwtSecret is not configured. Please set it in your appsettings.json or environment variables.");
        }

        var key = Encoding.ASCII.GetBytes(supabaseJwtSecret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set to true in production with HTTPS
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false, // Disable issuer validation for Supabase tokens
                ValidateAudience = true,
                ValidAudience = "authenticated",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // Add custom claims after token validation
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    try
                    {
                        // Get user ID from token - use ClaimTypes.NameIdentifier which is mapped from "sub"
                        var userIdClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                        {
                            return;
                        }

                        // Get DbContext from DI
                        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

                        // Get user from database
                        var user = await dbContext.Users
                            .Where(u => u.Id == userId)
                            .FirstOrDefaultAsync();

                        if (user == null)
                        {
                            return;
                        }

                        // Create a new ClaimsIdentity with the user's role from database
                        var claims = new List<System.Security.Claims.Claim>
                        {
                            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString())
                        };

                        // Add the new identity to the principal
                        // This will add the role claim while keeping existing claims from the JWT
                        var appIdentity = new System.Security.Claims.ClaimsIdentity(claims, "Application");
                        context.Principal?.AddIdentity(appIdentity);
                    }
                    catch (Exception)
                    {
                        // Don't fail authentication if role claim addition fails
                        // The endpoint authorization will handle missing role
                    }
                }
            };
        });

        // Register authorization handler
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Configure authorization policies
        services.AddAuthorization(options =>
        {
            // Define all permission-based policies
            var permissions = new[]
            {
                "requests.view",
                "requests.create",
                "requests.approve",
                "requests.manage_templates",
                "admin.users.view",
                "admin.users.manage",
                "admin.roles.view",
                "admin.roles.manage",
                "admin.settings.view",
                "admin.settings.manage"
            };

            foreach (var permission in permissions)
            {
                options.AddPolicy($"RequirePermission:{permission}", policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });
    }
}
