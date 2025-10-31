using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PortalForge.Application.Extensions;
using PortalForge.Application.Services;
using System.Reflection;

namespace PortalForge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add AutoMapper
        services.AddAutoMapper(assembly);

        // Add MediatR
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
        });

        // Register validators automatically
        services.AddValidators(assembly);

        // Register application services
        services.AddScoped<IRequestRoutingService, RequestRoutingService>();

        return services;
    }
}
