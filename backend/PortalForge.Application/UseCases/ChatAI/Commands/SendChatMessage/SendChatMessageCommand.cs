using MediatR;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.UseCases.ChatAI.Commands.SendChatMessage;

/// <summary>
/// Command to send a chat message to OpenAI.
/// Returns the complete AI response.
/// </summary>
public class SendChatMessageCommand : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
    public List<ChatMessage>? ConversationHistory { get; set; }
}
