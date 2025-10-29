using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.News.Commands.CreateNews;
using PortalForge.Application.UseCases.News.Commands.DeleteNews;
using PortalForge.Application.UseCases.News.Commands.SeedNewsData;
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
    public async Task<ActionResult<PaginatedNewsResponse>> GetAll(
        [FromQuery] string? category = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAllNewsQuery
        {
            Category = category,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
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
    [AllowAnonymous] // Disabled for testing
    public async Task<ActionResult<int>> Create([FromBody] CreateNewsRequestDto request)
    {
        // Try to get user ID from token, otherwise use default author (first user in system)
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid authorId;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out authorId))
        {
            // Use default author ID for testing (you should have this user in your system)
            // This is the ID from your seed data
            authorId = Guid.Parse("4f32c7f8-6e4d-4873-95e2-1547ce830768");
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
    [AllowAnonymous] // Disabled for testing
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
    [AllowAnonymous] // Disabled for testing
    public async Task<ActionResult> Delete(int id)
    {
        var command = new DeleteNewsCommand { NewsId = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("seed")]
    [AllowAnonymous]
    public async Task<ActionResult<int>> SeedData()
    {
        var command = new SeedNewsDataCommand();
        var count = await _mediator.Send(command);
        return Ok(new { message = $"Seeded {count} news articles", count });
    }
}
