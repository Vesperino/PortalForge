using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PortalForge.Application.Common.Interfaces;
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
        // Register handlers from both Application and Infrastructure assemblies
        services.AddMediatR(assembly, Assembly.Load("PortalForge.Infrastructure"));

        // Register pipeline behaviors
        // ValidationBehavior runs first to validate the request before any other processing
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PortalForge.Application.Common.Behaviors.ValidationBehavior<,>));
        // AuthorizationBehavior runs second to check if the user is authorized
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PortalForge.Application.Common.Behaviors.AuthorizationBehavior<,>));

        // Register validators automatically
        services.AddValidators(assembly);

        // Register application services
        services.AddScoped<IRequestRoutingService, RequestRoutingService>();
        services.AddScoped<IVacationCreationService, VacationCreationService>();
        services.AddScoped<IVacationStatusService, VacationStatusService>();
        services.AddScoped<IVacationSubstituteService, VacationSubstituteService>();

        return services;
    }
}
