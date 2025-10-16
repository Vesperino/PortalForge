using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Infrastructure.Auth;
using PortalForge.Infrastructure.Email;
using PortalForge.Infrastructure.Email.Models;
using PortalForge.Infrastructure.Persistence;
using PortalForge.Infrastructure.Repositories;
using PortalForge.Infrastructure.Validation;

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

        return services;
    }
}
