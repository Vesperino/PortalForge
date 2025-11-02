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

namespace PortalForge.Api.Controllers;

public class NewsController : BaseController
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
        [FromQuery] int? departmentId = null,
        [FromQuery] bool? isEvent = null,
        [FromQuery] string? hashtags = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAllNewsQuery
        {
            Category = category,
            DepartmentId = departmentId,
            IsEvent = isEvent,
            Hashtags = hashtags,
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
    public async Task<ActionResult<int>> Create([FromBody] CreateNewsRequestDto request)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var authorId);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var command = new CreateNewsCommand
        {
            Title = request.Title,
            Content = request.Content,
            Excerpt = request.Excerpt,
            ImageUrl = request.ImageUrl,
            AuthorId = authorId,
            Category = request.Category,
            EventId = request.EventId,
            IsEvent = request.IsEvent,
            EventHashtag = request.EventHashtag,
            EventDateTime = request.EventDateTime,
            EventLocation = request.EventLocation,
            EventPlaceId = request.EventPlaceId,
            DepartmentId = request.DepartmentId
        };

        var newsId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = newsId}, newsId);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateNewsRequestDto request)
    {
        var command = new UpdateNewsCommand
        {
            NewsId = id,
            Title = request.Title,
            Content = request.Content,
            Excerpt = request.Excerpt,
            ImageUrl = request.ImageUrl,
            Category = request.Category,
            EventId = request.EventId,
            IsEvent = request.IsEvent,
            EventHashtag = request.EventHashtag,
            EventDateTime = request.EventDateTime,
            EventLocation = request.EventLocation,
            EventPlaceId = request.EventPlaceId,
            DepartmentId = request.DepartmentId
        };

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var command = new DeleteNewsCommand { NewsId = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("seed")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> SeedData()
    {
        var command = new SeedNewsDataCommand();
        var count = await _mediator.Send(command);
        return Ok(new { message = $"Seeded {count} news articles", count });
    }
}
