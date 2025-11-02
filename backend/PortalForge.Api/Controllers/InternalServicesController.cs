using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.InternalServices.Commands.CreateService;
using PortalForge.Application.UseCases.InternalServices.Commands.UpdateService;
using PortalForge.Application.UseCases.InternalServices.Commands.DeleteService;
using PortalForge.Application.UseCases.InternalServices.Commands.CreateCategory;
using PortalForge.Application.UseCases.InternalServices.Commands.DeleteCategory;
using PortalForge.Application.UseCases.InternalServices.DTOs;
using PortalForge.Application.UseCases.InternalServices.Queries.GetAllServices;
using PortalForge.Application.UseCases.InternalServices.Queries.GetServiceById;
using PortalForge.Application.UseCases.InternalServices.Queries.GetServicesForUser;
using PortalForge.Application.UseCases.InternalServices.Queries.GetAllCategories;

namespace PortalForge.Api.Controllers;

[Authorize]
[Route("api/internal-services")]
public class InternalServicesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<InternalServicesController> _logger;

    public InternalServicesController(IMediator mediator, ILogger<InternalServicesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all internal services (requires internal_services.manage permission).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequirePermission:internal_services.manage")]
    public async Task<ActionResult<List<InternalServiceDto>>> GetAll()
    {
        var query = new GetAllServicesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get internal services for the current user (based on department).
    /// </summary>
    [HttpGet("my-services")]
    public async Task<ActionResult<List<InternalServiceDto>>> GetMyServices()
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var query = new GetServicesForUserQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get internal service by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InternalServiceDto>> GetById(Guid id)
    {
        var query = new GetServiceByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound($"Service with ID {id} not found");
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new internal service (requires internal_services.manage permission).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequirePermission:internal_services.manage")]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateInternalServiceRequestDto request)
    {
        var unauthorizedResult = GetUserIdOrUnauthorized(out var creatorId);
        if (unauthorizedResult != null)
        {
            return unauthorizedResult;
        }

        var command = new CreateServiceCommand
        {
            Name = request.Name,
            Description = request.Description,
            Url = request.Url,
            Icon = request.Icon,
            IconType = request.IconType,
            CategoryId = request.CategoryId,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            IsGlobal = request.IsGlobal,
            IsPinned = request.IsPinned,
            CreatedById = creatorId,
            DepartmentIds = request.DepartmentIds
        };

        var serviceId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = serviceId }, serviceId);
    }

    /// <summary>
    /// Update an internal service (requires internal_services.manage permission).
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequirePermission:internal_services.manage")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateInternalServiceRequestDto request)
    {
        if (id != request.Id)
        {
            return BadRequest("ID mismatch");
        }

        var command = new UpdateServiceCommand
        {
            Id = request.Id,
            Name = request.Name,
            Description = request.Description,
            Url = request.Url,
            Icon = request.Icon,
            IconType = request.IconType,
            CategoryId = request.CategoryId,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            IsGlobal = request.IsGlobal,
            IsPinned = request.IsPinned,
            DepartmentIds = request.DepartmentIds
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete an internal service (requires internal_services.manage permission).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequirePermission:internal_services.manage")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var command = new DeleteServiceCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ========== CATEGORIES ==========

    /// <summary>
    /// Get all categories.
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<List<InternalServiceCategoryDto>>> GetAllCategories()
    {
        var query = new GetAllCategoriesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new category (requires internal_services.manage permission).
    /// </summary>
    [HttpPost("categories")]
    [Authorize(Policy = "RequirePermission:internal_services.manage")]
    public async Task<ActionResult<Guid>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var command = new CreateCategoryCommand
        {
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon,
            DisplayOrder = request.DisplayOrder
        };

        var categoryId = await _mediator.Send(command);
        return Ok(categoryId);
    }

    /// <summary>
    /// Delete a category (requires internal_services.manage permission).
    /// </summary>
    [HttpDelete("categories/{id}")]
    [Authorize(Policy = "RequirePermission:internal_services.manage")]
    public async Task<ActionResult> DeleteCategory(Guid id)
    {
        var command = new DeleteCategoryCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
