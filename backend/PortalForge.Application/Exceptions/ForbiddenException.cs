using System.Net;

namespace PortalForge.Application.Exceptions;

public class ForbiddenException : CustomException
{
    public ForbiddenException(
        string message = "You are not allowed to do that action",
        List<string>? errors = null,
        HttpStatusCode statusCode = HttpStatusCode.Forbidden)
        : base(message, errors, statusCode)
    {
    }
}
