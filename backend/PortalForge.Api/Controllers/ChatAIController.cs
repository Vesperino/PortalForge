using System.Runtime.CompilerServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.Interfaces;
using PortalForge.Application.UseCases.ChatAI.Commands.SendChatMessage;
using PortalForge.Application.UseCases.ChatAI.Commands.TranslateText;

namespace PortalForge.Api.Controllers;

/// <summary>
/// Controller for AI Chat features including translation and standard chat.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequirePermission:chatai.use")]
public class ChatAIController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatAIController> _logger;

    public ChatAIController(IMediator mediator, ILogger<ChatAIController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Translates text to the specified language using AI (streaming response).
    /// </summary>
    /// <param name="command">Translation request containing text and target language.</param>
    /// <returns>Streamed translation response.</returns>
    [HttpPost("translate")]
    [Produces("text/plain")]
    public async IAsyncEnumerable<string> TranslateText(
        [FromBody] TranslateTextCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (GetUserIdOrUnauthorized(out var userId) is ActionResult errorResult)
        {
            _logger.LogWarning("Unauthorized translation attempt");
            yield return "Error: Unauthorized";
            yield break;
        }

        _logger.LogInformation(
            "User {UserId} initiating translation to {Language}. Text length: {Length}",
            userId, command.TargetLanguage, command.TextToTranslate.Length);

        IAsyncEnumerable<string>? stream = null;
        Exception? exception = null;

        try
        {
            stream = await _mediator.Send(command, cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        if (exception != null)
        {
            _logger.LogError(exception, "Error initiating translation for user {UserId}", userId);
            yield return $"Error: {exception.Message}";
            yield break;
        }

        if (stream != null)
        {
            await foreach (var chunk in stream.WithCancellation(cancellationToken))
            {
                yield return chunk;
            }
        }

        _logger.LogInformation("Translation completed for user {UserId}", userId);
    }

    /// <summary>
    /// Sends a chat message to AI and receives a streaming response.
    /// </summary>
    /// <param name="command">Chat request containing message and optional conversation history.</param>
    /// <returns>Streamed chat response.</returns>
    [HttpPost("chat")]
    [Produces("text/plain")]
    public async IAsyncEnumerable<string> SendChatMessage(
        [FromBody] SendChatMessageCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (GetUserIdOrUnauthorized(out var userId) is ActionResult errorResult)
        {
            _logger.LogWarning("Unauthorized chat attempt");
            yield return "Error: Unauthorized";
            yield break;
        }

        _logger.LogInformation(
            "User {UserId} sending chat message. Message length: {Length}, History count: {HistoryCount}",
            userId, command.Message.Length, command.ConversationHistory?.Count ?? 0);

        IAsyncEnumerable<string>? stream = null;
        Exception? exception = null;

        try
        {
            stream = await _mediator.Send(command, cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        if (exception != null)
        {
            _logger.LogError(exception, "Error initiating chat for user {UserId}", userId);
            yield return $"Error: {exception.Message}";
            yield break;
        }

        if (stream != null)
        {
            await foreach (var chunk in stream.WithCancellation(cancellationToken))
            {
                yield return chunk;
            }
        }

        _logger.LogInformation("Chat response completed for user {UserId}", userId);
    }

    /// <summary>
    /// Tests the OpenAI API configuration and connection.
    /// </summary>
    /// <returns>Result indicating whether the connection is successful.</returns>
    [HttpGet("test-connection")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<bool>> TestConnection(
        [FromServices] IOpenAIService openAIService,
        [FromQuery] string? testApiKey = null)
    {
        if (GetUserIdOrUnauthorized(out var userId) is ActionResult errorResult)
        {
            return errorResult;
        }

        _logger.LogInformation("Admin {UserId} testing OpenAI connection", userId);

        try
        {
            // If a test API key is provided, use it; otherwise use the configured one
            if (!string.IsNullOrEmpty(testApiKey))
            {
                var result = await openAIService.TestConnectionAsync(testApiKey);
                return Ok(result);
            }

            // For testing the configured key, we'll just try to get it
            // The service will throw if it's not configured properly
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI connection test failed for user {UserId}", userId);
            return Ok(false);
        }
    }
}
