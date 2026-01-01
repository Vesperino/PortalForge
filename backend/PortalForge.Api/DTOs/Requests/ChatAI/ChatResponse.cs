namespace PortalForge.Api.DTOs.Requests.ChatAI;

public sealed class ChatResponse
{
    public string Message { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
