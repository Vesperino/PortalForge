using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using Xunit;
using IFileStorageService = PortalForge.Application.Common.Interfaces.IFileStorageService;

namespace PortalForge.Tests.Unit.Application.Services;

/// <summary>
/// Unit tests for FormBuilderService.
/// Tests form generation, validation, conditional logic, and auto-complete functionality.
/// </summary>
public class FormBuilderServiceTests
{
    private readonly Mock<IRequestTemplateRepository> _templateRepositoryMock;
    private readonly Mock<IUnifiedValidatorService> _validatorServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<FormBuilderService>> _loggerMock;
    private readonly FormBuilderService _service;

    public FormBuilderServiceTests()
    {
        _templateRepositoryMock = new Mock<IRequestTemplateRepository>();
        _validatorServiceMock = new Mock<IUnifiedValidatorService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _loggerMock = new Mock<ILogger<FormBuilderService>>();
        
        _service = new FormBuilderService(
            _templateRepositoryMock.Object,
            _validatorServiceMock.Object,
            _fileStorageServiceMock.Object,
            _loggerMock.Object);
    }

    #region BuildFormAsync Tests

    [Fact]
    public async Task BuildFormAsync_WithValidTemplate_ReturnsFormDefinition()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = CreateSampleTemplate(templateId);
        
        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.BuildFormAsync(templateId);

        // Assert
        result.Should().NotBeNull();
        result.TemplateId.Should().Be(templateId);
        result.TemplateName.Should().Be(template.Name);
        result.Description.Should().Be(template.Description);
        result.AllowsAttachments.Should().Be(template.AllowsAttachments);
        result.Fields.Should().HaveCount(3);
        
        // Verify field order
        result.Fields.Should().BeInAscendingOrder(f => f.Order);
    }

    [Fact]
    public async Task BuildFormAsync_WithNonExistentTemplate_ThrowsArgumentException()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync((RequestTemplate?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.BuildFormAsync(templateId));
    }

    [Fact]
    public async Task BuildFormAsync_WithValidationRulesJson_ParsesValidationRules()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var validationRules = new List<ValidationRuleDto>
        {
            new() { Type = ValidationType.Required, Message = "This field is required" },
            new() { Type = ValidationType.MinLength, Value = "5", Message = "Minimum 5 characters" }
        };
        
        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "Test Template",
            Description = "Test Description",
            AllowsAttachments = false,
            Fields = new List<RequestTemplateField>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Test Field",
                    FieldType = FieldType.Text,
                    Order = 1,
                    ValidationRules = JsonSerializer.Serialize(validationRules)
                }
            }
        };

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.BuildFormAsync(templateId);

        // Assert
        result.Fields.First().ValidationRules.Should().HaveCount(2);
        result.Fields.First().ValidationRules.First().Type.Should().Be(ValidationType.Required);
        result.Fields.First().ValidationRules.Last().Type.Should().Be(ValidationType.MinLength);
    }

    [Fact]
    public async Task BuildFormAsync_WithConditionalLogicJson_ParsesConditionalLogic()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var conditionalLogic = new List<ConditionalLogicDto>
        {
            new() { FieldId = "field1", Operator = "equals", Value = "yes", Action = "show" }
        };
        
        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "Test Template",
            Description = "Test Description",
            AllowsAttachments = false,
            Fields = new List<RequestTemplateField>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Conditional Field",
                    FieldType = FieldType.Text,
                    Order = 1,
                    IsConditional = true,
                    ConditionalLogic = JsonSerializer.Serialize(conditionalLogic)
                }
            }
        };

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.BuildFormAsync(templateId);

        // Assert
        result.Fields.First().ConditionalLogic.Should().HaveCount(1);
        result.Fields.First().ConditionalLogic.First().FieldId.Should().Be("field1");
        result.Fields.First().ConditionalLogic.First().Operator.Should().Be("equals");
    }

    #endregion

    #region ValidateFormDataAsync Tests

    [Fact]
    public async Task ValidateFormDataAsync_WithValidData_ReturnsValidResult()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = CreateSampleTemplate(templateId);
        var formData = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            { template.Fields.First().Id.ToString(), "Valid text" },
            { template.Fields.Skip(1).First().Id.ToString(), "25" },
            { template.Fields.Last().Id.ToString(), "2024-01-01" }
        });

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.ValidateFormDataAsync(templateId, formData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateFormDataAsync_WithMissingRequiredField_ReturnsInvalidResult()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = CreateSampleTemplate(templateId);
        var formData = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            // Missing required text field
            { template.Fields.Skip(1).First().Id.ToString(), "25" }
        });

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.ValidateFormDataAsync(templateId, formData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Type.Should().Be(ValidationType.Required);
    }

    [Fact]
    public async Task ValidateFormDataAsync_WithInvalidNumber_ReturnsInvalidResult()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = CreateSampleTemplate(templateId);
        var formData = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            { template.Fields.First().Id.ToString(), "Valid text" },
            { template.Fields.Skip(1).First().Id.ToString(), "not_a_number" }, // Invalid number
            { template.Fields.Last().Id.ToString(), "2024-01-01" }
        });

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.ValidateFormDataAsync(templateId, formData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Type.Should().Be(ValidationType.Number);
    }

    [Fact]
    public async Task ValidateFormDataAsync_WithInvalidDate_ReturnsInvalidResult()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = CreateSampleTemplate(templateId);
        var formData = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            { template.Fields.First().Id.ToString(), "Valid text" },
            { template.Fields.Skip(1).First().Id.ToString(), "25" },
            { template.Fields.Last().Id.ToString(), "invalid_date" } // Invalid date
        });

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.ValidateFormDataAsync(templateId, formData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Type.Should().Be(ValidationType.Date);
    }

    [Fact]
    public async Task ValidateFormDataAsync_WithNonExistentTemplate_ReturnsInvalidResult()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync((RequestTemplate?)null);

        // Act
        var result = await _service.ValidateFormDataAsync(templateId, "{}");

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().FieldId.Should().Be("template");
    }

    [Fact]
    public async Task ValidateFormDataAsync_WithInvalidJson_ReturnsInvalidResult()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = CreateSampleTemplate(templateId);
        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.ValidateFormDataAsync(templateId, "invalid json");

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().FieldId.Should().Be("formData");
        result.Errors.First().Type.Should().Be(ValidationType.Custom);
    }

    #endregion

    #region ProcessConditionalLogicAsync Tests

    [Fact]
    public async Task ProcessConditionalLogicAsync_WithMatchingCondition_ReturnsVisibleField()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var triggerFieldId = Guid.NewGuid();
        
        var conditionalLogic = new List<ConditionalLogicDto>
        {
            new() { FieldId = triggerFieldId.ToString(), Operator = "equals", Value = "yes", Action = "show" }
        };

        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "Test Template",
            Description = "Test Description",
            AllowsAttachments = false,
            Fields = new List<RequestTemplateField>
            {
                new()
                {
                    Id = triggerFieldId,
                    Label = "Trigger Field",
                    FieldType = FieldType.Select,
                    Order = 1
                },
                new()
                {
                    Id = fieldId,
                    Label = "Conditional Field",
                    FieldType = FieldType.Text,
                    Order = 2,
                    IsConditional = true,
                    IsRequired = true,
                    ConditionalLogic = JsonSerializer.Serialize(conditionalLogic)
                }
            }
        };

        var formData = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            { triggerFieldId.ToString(), "yes" }
        });

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.ProcessConditionalLogicAsync(templateId, formData);

        // Assert
        var visibility = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result);
        visibility.Should().ContainKey(fieldId.ToString());
        
        var fieldVisibility = visibility![fieldId.ToString()];
        fieldVisibility.GetProperty("visible").GetBoolean().Should().BeTrue();
        fieldVisibility.GetProperty("required").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task ProcessConditionalLogicAsync_WithNonMatchingCondition_ReturnsHiddenField()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var triggerFieldId = Guid.NewGuid();
        
        var conditionalLogic = new List<ConditionalLogicDto>
        {
            new() { FieldId = triggerFieldId.ToString(), Operator = "equals", Value = "yes", Action = "show" }
        };

        var template = new RequestTemplate
        {
            Id = templateId,
            Name = "Test Template",
            Description = "Test Description",
            AllowsAttachments = false,
            Fields = new List<RequestTemplateField>
            {
                new()
                {
                    Id = triggerFieldId,
                    Label = "Trigger Field",
                    FieldType = FieldType.Select,
                    Order = 1
                },
                new()
                {
                    Id = fieldId,
                    Label = "Conditional Field",
                    FieldType = FieldType.Text,
                    Order = 2,
                    IsConditional = true,
                    IsRequired = true,
                    ConditionalLogic = JsonSerializer.Serialize(conditionalLogic)
                }
            }
        };

        var formData = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            { triggerFieldId.ToString(), "no" } // Different value
        });

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.ProcessConditionalLogicAsync(templateId, formData);

        // Assert
        var visibility = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result);
        visibility.Should().ContainKey(fieldId.ToString());
        
        var fieldVisibility = visibility![fieldId.ToString()];
        fieldVisibility.GetProperty("visible").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task ProcessConditionalLogicAsync_WithNonExistentTemplate_ThrowsArgumentException()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync((RequestTemplate?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.ProcessConditionalLogicAsync(templateId, "{}"));
    }

    #endregion

    #region GetAutoCompleteOptionsAsync Tests

    [Fact]
    public async Task GetAutoCompleteOptionsAsync_WithUsersSource_ReturnsUserOptions()
    {
        // Act
        var result = await _service.GetAutoCompleteOptionsAsync("users", "john");

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(o => o.Label.Contains("John", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetAutoCompleteOptionsAsync_WithDepartmentsSource_ReturnsDepartmentOptions()
    {
        // Act
        var result = await _service.GetAutoCompleteOptionsAsync("departments", "information");

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(o => o.Label.Contains("Information Technology", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetAutoCompleteOptionsAsync_WithCountriesSource_ReturnsCountryOptions()
    {
        // Act
        var result = await _service.GetAutoCompleteOptionsAsync("countries", "pol");

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(o => o.Label.Contains("Poland", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetAutoCompleteOptionsAsync_WithUnknownSource_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetAutoCompleteOptionsAsync("unknown", "query");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAutoCompleteOptionsAsync_WithEmptyQuery_ReturnsAllOptions()
    {
        // Act
        var result = await _service.GetAutoCompleteOptionsAsync("users", "");

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCountGreaterThan(1);
    }

    #endregion

    #region Helper Methods

    private RequestTemplate CreateSampleTemplate(Guid templateId)
    {
        return new RequestTemplate
        {
            Id = templateId,
            Name = "Sample Template",
            Description = "A sample template for testing",
            AllowsAttachments = true,
            Fields = new List<RequestTemplateField>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Text Field",
                    FieldType = FieldType.Text,
                    IsRequired = true,
                    Order = 1
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Number Field",
                    FieldType = FieldType.Number,
                    IsRequired = false,
                    MinValue = 1,
                    MaxValue = 100,
                    Order = 2
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Date Field",
                    FieldType = FieldType.Date,
                    IsRequired = false,
                    Order = 3
                }
            }
        };
    }

    #endregion
}