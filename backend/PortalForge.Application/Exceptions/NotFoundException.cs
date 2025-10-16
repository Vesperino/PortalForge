using System.Net;

namespace PortalForge.Application.Exceptions;

public class NotFoundException : CustomException
{
    public NotFoundException(
        string message = "Resource not found",
        List<string>? errors = null,
        HttpStatusCode statusCode = HttpStatusCode.NotFound)
        : base(message, errors, statusCode)
    {
    }
}
