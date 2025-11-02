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

        // Add MediatR (v11.x syntax)
        services.AddMediatR(assembly);

        // Register validators automatically
        services.AddValidators(assembly);

        // Register application services
        services.AddScoped<IRequestRoutingService, RequestRoutingService>();
        services.AddScoped<IVacationScheduleService, VacationScheduleService>();

        return services;
    }
}
