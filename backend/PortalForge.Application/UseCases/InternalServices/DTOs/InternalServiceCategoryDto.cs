namespace PortalForge.Application.UseCases.InternalServices.DTOs;

public class InternalServiceCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public List<InternalServiceDto> Services { get; set; } = new();
}
