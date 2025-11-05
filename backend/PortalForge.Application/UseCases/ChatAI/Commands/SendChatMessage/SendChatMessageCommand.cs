using MediatR;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.UseCases.ChatAI.Commands.SendChatMessage;

/// <summary>
/// Command to send a chat message to OpenAI.
/// Returns an async enumerable for streaming the response.
/// </summary>
public class SendChatMessageCommand : IRequest<IAsyncEnumerable<string>>
{
    public string Message { get; set; } = string.Empty;
    public List<ChatMessage>? ConversationHistory { get; set; }
}
