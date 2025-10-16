using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Infrastructure.Validation;

public class UnifiedValidatorService : IUnifiedValidatorService
{
    private readonly IServiceProvider _serviceProvider;

    public UnifiedValidatorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ValidateAsync<T>(T instance) where T : class
    {
        var validatorType = typeof(IValidator<T>);
        var validator = _serviceProvider.GetService(validatorType) as IValidator<T>;

        if (validator == null)
        {
            // No validator registered for this type - continue without validation
            return;
        }

        var validationResult = await validator.ValidateAsync(instance);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
