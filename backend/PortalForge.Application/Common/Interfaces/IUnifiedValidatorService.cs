namespace PortalForge.Application.Common.Interfaces;

public interface IUnifiedValidatorService
{
    Task ValidateAsync<T>(T instance) where T : class;
}
