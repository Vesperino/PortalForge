using System.Net;

namespace PortalForge.Application.Exceptions;

public class ValidationException : CustomException
{
    public ValidationException(
        string message = "Validation failed",
        List<string>? errors = null,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message, errors, statusCode)
    {
    }
}
