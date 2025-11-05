namespace PortalForge.Application.Interfaces;

/// <summary>
/// Service for integrating with OpenAI API for chat and translation features.
/// </summary>
public interface IOpenAIService
{
    /// <summary>
    /// Sends a translation request to OpenAI and streams the response.
    /// </summary>
    /// <param name="textToTranslate">The text to translate.</param>
    /// <param name="targetLanguage">The target language for translation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable stream of response chunks.</returns>
    IAsyncEnumerable<string> TranslateTextStreamAsync(
        string textToTranslate,
        string targetLanguage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a chat message to OpenAI and streams the response.
    /// </summary>
    /// <param name="message">The user's message.</param>
    /// <param name="conversationHistory">Optional previous messages for context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable stream of response chunks.</returns>
    IAsyncEnumerable<string> ChatStreamAsync(
        string message,
        IEnumerable<ChatMessage>? conversationHistory = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the OpenAI API connection with the provided API key.
    /// </summary>
    /// <param name="apiKey">The OpenAI API key to test.</param>
    /// <returns>True if the connection is successful, otherwise false.</returns>
    Task<bool> TestConnectionAsync(string apiKey);
}

/// <summary>
/// Represents a chat message with role and content.
/// </summary>
public class ChatMessage
{
    public string Role { get; set; } = string.Empty; // "system", "user", or "assistant"
    public string Content { get; set; } = string.Empty;
}
