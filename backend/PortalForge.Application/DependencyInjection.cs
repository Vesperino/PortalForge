using Microsoft.Extensions.DependencyInjection;
using PortalForge.Application.Extensions;
using System.Reflection;

namespace PortalForge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register validators automatically
        services.AddValidators(Assembly.GetExecutingAssembly());

        return services;
    }
}
