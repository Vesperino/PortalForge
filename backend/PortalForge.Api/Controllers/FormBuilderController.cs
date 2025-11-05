using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Api.DTOs.Requests.FormBuilder;
using PortalForge.Application.Interfaces;

namespace PortalForge.Api.Controllers;

[Route("api/form-builder")]
[Authorize]
public class FormBuilderController : BaseController
{
    private readonly IFormBuilderService _formBuilderService;
    private readonly ILogger<FormBuilderController> _logger;

    public FormBuilderController(
        IFormBuilderService formBuilderService,
        ILogger<FormBuilderController> logger)
    {
        _formBuilderService = formBuilderService;
        _logger = logger;
    }

    /// <summary>
    /// Build form definition from request template
    /// </summary>
    [HttpGet("templates/{templateId:guid}/form")]
    public async Task<ActionResult> BuildForm(Guid templateId)
    {
        try
        {
            var result = await _formBuilderService.BuildFormAsync(templateId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building form for template {TemplateId}", templateId);
            return BadRequest(new { Message = "Failed to build form definition" });
        }
    }

    /// <summary>
    /// Validate form data against template rules
    /// </summary>
    [HttpPost("templates/{templateId:guid}/validate")]
    public async Task<ActionResult> ValidateFormData(
        Guid templateId, 
        [FromBody] ValidateFormDataDto dto)
    {
        try
        {
            var result = await _formBuilderService.ValidateFormDataAsync(templateId, dto.FormData);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating form data for template {TemplateId}", templateId);
            return BadRequest(new { Message = "Failed to validate form data" });
        }
    }

    /// <summary>
    /// Process conditional logic for field visibility
    /// </summary>
    [HttpPost("templates/{templateId:guid}/conditional-logic")]
    public async Task<ActionResult> ProcessConditionalLogic(
        Guid templateId, 
        [FromBody] ProcessConditionalLogicDto dto)
    {
        try
        {
            var result = await _formBuilderService.ProcessConditionalLogicAsync(templateId, dto.FormData);
            return Ok(new { FieldVisibility = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing conditional logic for template {TemplateId}", templateId);
            return BadRequest(new { Message = "Failed to process conditional logic" });
        }
    }

    /// <summary>
    /// Get auto-complete options for intelligent field suggestions
    /// </summary>
    [HttpGet("autocomplete/{source}")]
    public async Task<ActionResult> GetAutoCompleteOptions(
        string source, 
        [FromQuery] string query = "")
    {
        try
        {
            var result = await _formBuilderService.GetAutoCompleteOptionsAsync(source, query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting auto-complete options for source {Source}", source);
            return BadRequest(new { Message = "Failed to get auto-complete options" });
        }
    }
}