using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Infrastructure.Auth;
using PortalForge.Infrastructure.Email;
using PortalForge.Infrastructure.Email.Models;
using PortalForge.Infrastructure.Persistence;
using PortalForge.Infrastructure.Repositories;
using PortalForge.Infrastructure.Validation;
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
        });

        services.AddAuthorization();
    }
}
