namespace PortalForge.Application.Common.Interfaces;

public interface IRequireAuthorization
{
    string[] RequiredRoles => Array.Empty<string>();
    bool RequireAuthenticatedUser => true;
}
