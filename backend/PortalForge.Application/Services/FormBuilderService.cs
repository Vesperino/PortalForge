using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Enums;
using IFileStorageService = PortalForge.Application.Common.Interfaces.IFileStorageService;

namespace PortalForge.Application.Services;

/// <summary>
/// Service for building dynamic forms from request templates and handling form validation
/// </summary>
public class FormBuilderService : IFormBuilderService
{
    private readonly IRequestTemplateRepository _templateRepository;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FormBuilderService> _logger;

    public FormBuilderService(
        IRequestTemplateRepository templateRepository,
        IUnifiedValidatorService validatorService,
        IFileStorageService fileStorageService,
        ILogger<FormBuilderService> logger)
    {
        _templateRepository = templateRepository;
        _validatorService = validatorService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<FormDefinitionDto> BuildFormAsync(Guid templateId)
    {
        _logger.LogInformation("Building form for template {TemplateId}", templateId);

        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null)
        {
            throw new ArgumentException($"Template with ID {templateId} not found");
        }

        var formDefinition = new FormDefinitionDto
        {
            TemplateId = template.Id,
            TemplateName = template.Name,
            Description = template.Description,
            AllowsAttachments = template.AllowsAttachments,
            Fields = new List<FormFieldDto>()
        };

        foreach (var field in template.Fields.OrderBy(f => f.Order))
        {
            var formField = new FormFieldDto
            {
                Id = field.Id,
                Label = field.Label,
                FieldType = field.FieldType,
                Placeholder = field.Placeholder,
                IsRequired = field.IsRequired,
                Options = field.Options,
                MinValue = field.MinValue,
                MaxValue = field.MaxValue,
                HelpText = field.HelpText,
                Order = field.Order,
                DefaultValue = field.DefaultValue,
                IsConditional = field.IsConditional,
                AutoCompleteSource = field.AutoCompleteSource,
                FileMaxSize = field.FileMaxSize,
                AllowedFileTypes = field.AllowedFileTypes
            };

            // Parse validation rules from JSON
            if (!string.IsNullOrEmpty(field.ValidationRules))
            {
                try
                {
                    formField.ValidationRules = JsonSerializer.Deserialize<List<ValidationRuleDto>>(field.ValidationRules) ?? new List<ValidationRuleDto>();
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse validation rules for field {FieldId}", field.Id);
                }
            }

            // Parse conditional logic from JSON
            if (!string.IsNullOrEmpty(field.ConditionalLogic))
            {
                try
                {
                    formField.ConditionalLogic = JsonSerializer.Deserialize<List<ConditionalLogicDto>>(field.ConditionalLogic) ?? new List<ConditionalLogicDto>();
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse conditional logic for field {FieldId}", field.Id);
                }
            }

            formDefinition.Fields.Add(formField);
        }

        _logger.LogInformation("Successfully built form with {FieldCount} fields for template {TemplateId}", 
            formDefinition.Fields.Count, templateId);

        return formDefinition;
    }

    public async Task<ValidationResult> ValidateFormDataAsync(Guid templateId, string formData)
    {
        _logger.LogInformation("Validating form data for template {TemplateId}", templateId);

        var result = new ValidationResult();
        
        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                FieldId = "template",
                Message = $"Template with ID {templateId} not found",
                Type = ValidationType.Required
            });
            return result;
        }

        Dictionary<string, object?> formValues;
        try
        {
            formValues = JsonSerializer.Deserialize<Dictionary<string, object?>>(formData) ?? new Dictionary<string, object?>();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse form data JSON");
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                FieldId = "formData",
                Message = "Invalid JSON format",
                Type = ValidationType.Custom
            });
            return result;
        }

        foreach (var field in template.Fields)
        {
            var fieldValue = formValues.ContainsKey(field.Id.ToString()) ? formValues[field.Id.ToString()] : null;
            var fieldErrors = await ValidateFieldAsync(field, fieldValue, formValues);
            result.Errors.AddRange(fieldErrors);
        }

        result.IsValid = !result.Errors.Any();
        
        _logger.LogInformation("Form validation completed for template {TemplateId}. Valid: {IsValid}, Errors: {ErrorCount}", 
            templateId, result.IsValid, result.Errors.Count);

        return result;
    }

    public async Task<string> ProcessConditionalLogicAsync(Guid templateId, string formData)
    {
        _logger.LogInformation("Processing conditional logic for template {TemplateId}", templateId);

        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null)
        {
            throw new ArgumentException($"Template with ID {templateId} not found");
        }

        Dictionary<string, object?> formValues;
        try
        {
            formValues = JsonSerializer.Deserialize<Dictionary<string, object?>>(formData) ?? new Dictionary<string, object?>();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse form data JSON for conditional logic");
            return "{}";
        }

        var fieldVisibility = new Dictionary<string, object>();

        foreach (var field in template.Fields.Where(f => f.IsConditional && !string.IsNullOrEmpty(f.ConditionalLogic)))
        {
            try
            {
                var conditionalRules = JsonSerializer.Deserialize<List<ConditionalLogicDto>>(field.ConditionalLogic!) ?? new List<ConditionalLogicDto>();
                var isVisible = EvaluateConditionalLogic(conditionalRules, formValues);
                
                fieldVisibility[field.Id.ToString()] = new
                {
                    visible = isVisible,
                    required = isVisible && field.IsRequired
                };
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse conditional logic for field {FieldId}", field.Id);
                // Default to visible if parsing fails
                fieldVisibility[field.Id.ToString()] = new { visible = true, required = field.IsRequired };
            }
        }

        var result = JsonSerializer.Serialize(fieldVisibility);
        _logger.LogInformation("Conditional logic processed for {FieldCount} fields", fieldVisibility.Count);
        
        return result;
    }

    public async Task<List<AutoCompleteOptionDto>> GetAutoCompleteOptionsAsync(string source, string query)
    {
        _logger.LogInformation("Getting auto-complete options for source {Source} with query '{Query}'", source, query);

        var options = new List<AutoCompleteOptionDto>();

        // Handle different auto-complete sources
        switch (source.ToLowerInvariant())
        {
            case "users":
                // This would typically query a user repository
                // For now, return mock data
                options.AddRange(GetMockUserOptions(query));
                break;
            
            case "departments":
                // This would typically query a department repository
                options.AddRange(GetMockDepartmentOptions(query));
                break;
            
            case "countries":
                options.AddRange(GetMockCountryOptions(query));
                break;
            
            default:
                _logger.LogWarning("Unknown auto-complete source: {Source}", source);
                break;
        }

        _logger.LogInformation("Returning {OptionCount} auto-complete options for source {Source}", options.Count, source);
        return options;
    }

    private async Task<List<ValidationError>> ValidateFieldAsync(Domain.Entities.RequestTemplateField field, object? value, Dictionary<string, object?> allValues)
    {
        var errors = new List<ValidationError>();
        var stringValue = value?.ToString() ?? string.Empty;

        // Required field validation
        if (field.IsRequired && (value == null || string.IsNullOrWhiteSpace(stringValue)))
        {
            errors.Add(new ValidationError
            {
                FieldId = field.Id.ToString(),
                Message = $"{field.Label} is required",
                Type = ValidationType.Required
            });
            return errors; // Don't validate further if required field is empty
        }

        // Skip validation if field is empty and not required
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return errors;
        }

        // Parse and apply custom validation rules
        if (!string.IsNullOrEmpty(field.ValidationRules))
        {
            try
            {
                var validationRules = JsonSerializer.Deserialize<List<ValidationRuleDto>>(field.ValidationRules) ?? new List<ValidationRuleDto>();
                foreach (var rule in validationRules)
                {
                    var ruleError = ValidateRule(field, stringValue, rule);
                    if (ruleError != null)
                    {
                        errors.Add(ruleError);
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse validation rules for field {FieldId}", field.Id);
            }
        }

        // Field type specific validation
        switch (field.FieldType)
        {
            case FieldType.Number:
                if (!int.TryParse(stringValue, out var numValue))
                {
                    errors.Add(new ValidationError
                    {
                        FieldId = field.Id.ToString(),
                        Message = $"{field.Label} must be a valid number",
                        Type = ValidationType.Number
                    });
                }
                else
                {
                    if (field.MinValue.HasValue && numValue < field.MinValue.Value)
                    {
                        errors.Add(new ValidationError
                        {
                            FieldId = field.Id.ToString(),
                            Message = $"{field.Label} must be at least {field.MinValue.Value}",
                            Type = ValidationType.Range
                        });
                    }
                    if (field.MaxValue.HasValue && numValue > field.MaxValue.Value)
                    {
                        errors.Add(new ValidationError
                        {
                            FieldId = field.Id.ToString(),
                            Message = $"{field.Label} must be at most {field.MaxValue.Value}",
                            Type = ValidationType.Range
                        });
                    }
                }
                break;

            case FieldType.Date:
                if (!DateTime.TryParse(stringValue, out _))
                {
                    errors.Add(new ValidationError
                    {
                        FieldId = field.Id.ToString(),
                        Message = $"{field.Label} must be a valid date",
                        Type = ValidationType.Date
                    });
                }
                break;

            case FieldType.FileUpload:
                // Validate file upload metadata if provided
                var fileValidationErrors = await ValidateFileUploadFieldAsync(field, stringValue);
                errors.AddRange(fileValidationErrors);
                break;
        }

        return errors;
    }

    private ValidationError? ValidateRule(Domain.Entities.RequestTemplateField field, string value, ValidationRuleDto rule)
    {
        switch (rule.Type)
        {
            case ValidationType.MinLength:
                if (int.TryParse(rule.Value, out var minLength) && value.Length < minLength)
                {
                    return new ValidationError
                    {
                        FieldId = field.Id.ToString(),
                        Message = rule.Message ?? $"{field.Label} must be at least {minLength} characters long",
                        Type = ValidationType.MinLength
                    };
                }
                break;

            case ValidationType.MaxLength:
                if (int.TryParse(rule.Value, out var maxLength) && value.Length > maxLength)
                {
                    return new ValidationError
                    {
                        FieldId = field.Id.ToString(),
                        Message = rule.Message ?? $"{field.Label} must be at most {maxLength} characters long",
                        Type = ValidationType.MaxLength
                    };
                }
                break;

            case ValidationType.Regex:
                if (!string.IsNullOrEmpty(rule.Value) && !Regex.IsMatch(value, rule.Value))
                {
                    return new ValidationError
                    {
                        FieldId = field.Id.ToString(),
                        Message = rule.Message ?? $"{field.Label} format is invalid",
                        Type = ValidationType.Regex
                    };
                }
                break;

            case ValidationType.Email:
                var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(value, emailRegex))
                {
                    return new ValidationError
                    {
                        FieldId = field.Id.ToString(),
                        Message = rule.Message ?? $"{field.Label} must be a valid email address",
                        Type = ValidationType.Email
                    };
                }
                break;

            case ValidationType.Phone:
                var phoneRegex = @"^\+?[\d\s\-\(\)]+$";
                if (!Regex.IsMatch(value, phoneRegex))
                {
                    return new ValidationError
                    {
                        FieldId = field.Id.ToString(),
                        Message = rule.Message ?? $"{field.Label} must be a valid phone number",
                        Type = ValidationType.Phone
                    };
                }
                break;

            case ValidationType.Url:
                if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                {
                    return new ValidationError
                    {
                        FieldId = field.Id.ToString(),
                        Message = rule.Message ?? $"{field.Label} must be a valid URL",
                        Type = ValidationType.Url
                    };
                }
                break;
        }

        return null;
    }

    private bool EvaluateConditionalLogic(List<ConditionalLogicDto> rules, Dictionary<string, object?> formValues)
    {
        if (!rules.Any())
        {
            return true; // Show field if no rules defined
        }

        // For now, implement simple AND logic - all rules must be satisfied
        foreach (var rule in rules)
        {
            if (!EvaluateRule(rule, formValues))
            {
                return false;
            }
        }

        return true;
    }

    private bool EvaluateRule(ConditionalLogicDto rule, Dictionary<string, object?> formValues)
    {
        if (!formValues.ContainsKey(rule.FieldId))
        {
            return false;
        }

        var fieldValue = formValues[rule.FieldId]?.ToString() ?? string.Empty;

        return rule.Operator.ToLowerInvariant() switch
        {
            "equals" => fieldValue.Equals(rule.Value, StringComparison.OrdinalIgnoreCase),
            "not_equals" => !fieldValue.Equals(rule.Value, StringComparison.OrdinalIgnoreCase),
            "contains" => fieldValue.Contains(rule.Value, StringComparison.OrdinalIgnoreCase),
            "not_contains" => !fieldValue.Contains(rule.Value, StringComparison.OrdinalIgnoreCase),
            "starts_with" => fieldValue.StartsWith(rule.Value, StringComparison.OrdinalIgnoreCase),
            "ends_with" => fieldValue.EndsWith(rule.Value, StringComparison.OrdinalIgnoreCase),
            "is_empty" => string.IsNullOrWhiteSpace(fieldValue),
            "is_not_empty" => !string.IsNullOrWhiteSpace(fieldValue),
            _ => false
        };
    }

    private List<AutoCompleteOptionDto> GetMockUserOptions(string query)
    {
        // This would typically query the user repository
        var mockUsers = new List<AutoCompleteOptionDto>
        {
            new() { Value = "user1", Label = "John Doe", Description = "Software Developer" },
            new() { Value = "user2", Label = "Jane Smith", Description = "Project Manager" },
            new() { Value = "user3", Label = "Bob Johnson", Description = "QA Engineer" }
        };

        return mockUsers
            .Where(u => string.IsNullOrEmpty(query) || 
                       u.Label.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                       u.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
            .Take(10)
            .ToList();
    }

    private List<AutoCompleteOptionDto> GetMockDepartmentOptions(string query)
    {
        var mockDepartments = new List<AutoCompleteOptionDto>
        {
            new() { Value = "it", Label = "Information Technology" },
            new() { Value = "hr", Label = "Human Resources" },
            new() { Value = "finance", Label = "Finance" },
            new() { Value = "marketing", Label = "Marketing" },
            new() { Value = "sales", Label = "Sales" }
        };

        return mockDepartments
            .Where(d => string.IsNullOrEmpty(query) || 
                       d.Label.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();
    }

    private List<AutoCompleteOptionDto> GetMockCountryOptions(string query)
    {
        var mockCountries = new List<AutoCompleteOptionDto>
        {
            new() { Value = "pl", Label = "Poland" },
            new() { Value = "us", Label = "United States" },
            new() { Value = "de", Label = "Germany" },
            new() { Value = "fr", Label = "France" },
            new() { Value = "uk", Label = "United Kingdom" }
        };

        return mockCountries
            .Where(c => string.IsNullOrEmpty(query) || 
                       c.Label.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();
    }

    private async Task<List<ValidationError>> ValidateFileUploadFieldAsync(Domain.Entities.RequestTemplateField field, string fileMetadata)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(fileMetadata))
        {
            return errors; // No file uploaded, validation handled by required field check
        }

        try
        {
            // Parse file metadata (assuming JSON format with file info)
            var fileInfo = JsonSerializer.Deserialize<FileUploadMetadata>(fileMetadata);
            if (fileInfo == null)
            {
                errors.Add(new ValidationError
                {
                    FieldId = field.Id.ToString(),
                    Message = "Invalid file metadata format",
                    Type = ValidationType.Custom
                });
                return errors;
            }

            // Validate file size
            if (field.FileMaxSize.HasValue && fileInfo.Size > field.FileMaxSize.Value)
            {
                errors.Add(new ValidationError
                {
                    FieldId = field.Id.ToString(),
                    Message = $"File size ({FormatFileSize(fileInfo.Size)}) exceeds maximum allowed size ({FormatFileSize(field.FileMaxSize.Value)})",
                    Type = ValidationType.FileSize
                });
            }

            // Validate file type
            if (!string.IsNullOrEmpty(field.AllowedFileTypes))
            {
                var allowedTypes = JsonSerializer.Deserialize<string[]>(field.AllowedFileTypes) ?? Array.Empty<string>();
                var fileExtension = Path.GetExtension(fileInfo.FileName).ToLowerInvariant();
                
                if (allowedTypes.Length > 0 && !allowedTypes.Contains(fileExtension))
                {
                    errors.Add(new ValidationError
                    {
                        FieldId = field.Id.ToString(),
                        Message = $"File type '{fileExtension}' is not allowed. Allowed types: {string.Join(", ", allowedTypes)}",
                        Type = ValidationType.FileType
                    });
                }
            }

            // Additional file validation using the file storage service
            if (!string.IsNullOrEmpty(fileInfo.TempPath) && await _fileStorageService.FileExistsAsync(fileInfo.TempPath))
            {
                // Basic file existence validation - more detailed validation would be done during actual file upload
                _logger.LogInformation("File exists at path: {TempPath}", fileInfo.TempPath);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse file metadata for field {FieldId}", field.Id);
            errors.Add(new ValidationError
            {
                FieldId = field.Id.ToString(),
                Message = "Invalid file metadata format",
                Type = ValidationType.Custom
            });
        }

        return errors;
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Validates form data using the existing validation infrastructure
    /// </summary>
    /// <param name="formData">The form data to validate</param>
    /// <param name="templateId">The template ID for context</param>
    /// <returns>True if validation passes, false otherwise</returns>
    public async Task<bool> ValidateWithInfrastructureAsync(object formData, Guid templateId)
    {
        try
        {
            await _validatorService.ValidateAsync(formData);
            return true;
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation failed for template {TemplateId}: {Errors}", 
                templateId, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return false;
        }
    }

    /// <summary>
    /// Metadata for uploaded files
    /// </summary>
    private class FileUploadMetadata
    {
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string? TempPath { get; set; }
        public string? ContentType { get; set; }
    }
}