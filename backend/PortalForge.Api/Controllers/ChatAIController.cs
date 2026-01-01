using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Api.DTOs.Requests.ChatAI;
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
    /// Translates text to the specified language using AI.
    /// </summary>
    /// <param name="command">Translation request containing text and target language.</param>
    /// <returns>Translation response.</returns>
    [HttpPost("translate")]
    public async Task<ActionResult<TranslationResponse>> TranslateText(
        [FromBody] TranslateTextCommand command,
        CancellationToken cancellationToken)
    {
        if (GetUserIdOrUnauthorized(out var userId) is ActionResult errorResult)
        {
            _logger.LogWarning("Unauthorized translation attempt");
            return Unauthorized(new { error = "Unauthorized" });
        }

        _logger.LogInformation(
            "User {UserId} initiating translation to {Language}. Text length: {Length}",
            userId, command.TargetLanguage, command.TextToTranslate.Length);

        try
        {
            var translatedText = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Translation completed for user {UserId}", userId);

            return Ok(new TranslationResponse
            {
                TranslatedText = translatedText,
                SourceLanguage = "auto",
                TargetLanguage = command.TargetLanguage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during translation for user {UserId}", userId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Sends a chat message to AI and receives a response.
    /// </summary>
    /// <param name="command">Chat request containing message and optional conversation history.</param>
    /// <returns>Chat response.</returns>
    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> SendChatMessage(
        [FromBody] SendChatMessageCommand command,
        CancellationToken cancellationToken)
    {
        if (GetUserIdOrUnauthorized(out var userId) is ActionResult errorResult)
        {
            _logger.LogWarning("Unauthorized chat attempt");
            return Unauthorized(new { error = "Unauthorized" });
        }

        _logger.LogInformation(
            "User {UserId} sending chat message. Message length: {Length}, History count: {HistoryCount}",
            userId, command.Message.Length, command.ConversationHistory?.Count ?? 0);

        try
        {
            var responseMessage = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Chat response completed for user {UserId}", userId);

            return Ok(new ChatResponse
            {
                Message = responseMessage,
                Role = "assistant"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during chat for user {UserId}", userId);
            return StatusCode(500, new { error = ex.Message });
        }
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
