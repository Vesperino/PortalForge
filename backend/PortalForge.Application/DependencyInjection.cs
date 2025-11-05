using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PortalForge.Application.Extensions;
using PortalForge.Application.Interfaces;
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

        // Register validators automatically
        services.AddValidators(assembly);

        // Register application services
        services.AddScoped<IRequestRoutingService, EnhancedRequestRoutingService>();
        services.AddScoped<IEnhancedRequestRoutingService, EnhancedRequestRoutingService>();
        services.AddScoped<IVacationScheduleService, VacationScheduleService>();
        services.AddScoped<IFormBuilderService, FormBuilderService>();
        services.AddScoped<IServiceRequestHandler, ServiceRequestHandler>();
        services.AddScoped<IServiceCategoryConfigService, ServiceCategoryConfigService>();

        return services;
    }
}
