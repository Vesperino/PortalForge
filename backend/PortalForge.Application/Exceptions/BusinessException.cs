using System.Net;

namespace PortalForge.Application.Exceptions;

/// <summary>
/// Exception for business rule violations that should be shown to users.
/// Use this for validation errors, constraint violations, and other user-facing errors.
/// </summary>
public class BusinessException : CustomException
{
    public BusinessException(
        string message,
        List<string>? errors = null,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message, errors, statusCode)
    {
    }
}
