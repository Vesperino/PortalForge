using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.News.Commands.CreateNews;
using PortalForge.Application.UseCases.News.Commands.DeleteNews;
using PortalForge.Application.UseCases.News.Commands.UpdateNews;
using PortalForge.Application.UseCases.News.DTOs;
using PortalForge.Application.UseCases.News.Queries.GetAllNews;
using PortalForge.Application.UseCases.News.Queries.GetNewsById;
using PortalForge.Domain.Entities;
using System.Security.Claims;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NewsController> _logger;

    public NewsController(IMediator mediator, ILogger<NewsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<NewsDto>>> GetAll([FromQuery] string? category = null)
    {
        var query = new GetAllNewsQuery { Category = category };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<NewsDto>> GetById(int id)
    {
        var query = new GetNewsByIdQuery { NewsId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Hr,Marketing")]
    public async Task<ActionResult<int>> Create([FromBody] CreateNewsRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var authorId))
        {
            return Unauthorized("User ID not found in token");
        }

        if (!Enum.TryParse<NewsCategory>(request.Category, true, out var category))
        {
            return BadRequest("Invalid category");
        }

        var command = new CreateNewsCommand
        {
            Title = request.Title,
            Content = request.Content,
            Excerpt = request.Excerpt,
            ImageUrl = request.ImageUrl,
            AuthorId = authorId,
            Category = category,
            EventId = request.EventId
        };

        var newsId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = newsId }, newsId);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Hr,Marketing")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateNewsRequestDto request)
    {
        if (!Enum.TryParse<NewsCategory>(request.Category, true, out var category))
        {
            return BadRequest("Invalid category");
        }

        var command = new UpdateNewsCommand
        {
            NewsId = id,
            Title = request.Title,
            Content = request.Content,
            Excerpt = request.Excerpt,
            ImageUrl = request.ImageUrl,
            Category = category,
            EventId = request.EventId
        };

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Hr,Marketing")]
    public async Task<ActionResult> Delete(int id)
    {
        var command = new DeleteNewsCommand { NewsId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
