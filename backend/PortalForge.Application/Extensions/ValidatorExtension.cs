using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace PortalForge.Application.Extensions;

public static class ValidatorExtension
{
    public static IServiceCollection AddValidators(
        this IServiceCollection services,
        Assembly assembly)
    {
        var validatorTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(t => t.Interface.IsGenericType &&
                       t.Interface.GetGenericTypeDefinition() == typeof(IValidator<>))
            .ToList();

        foreach (var validator in validatorTypes)
        {
            services.AddTransient(validator.Interface, validator.Type);
        }

        return services;
    }
}
