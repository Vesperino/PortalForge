namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a company event in the portal.
/// </summary>
public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }

    public List<EventTag> Tags { get; set; } = new();
    public List<int> TargetDepartments { get; set; } = new();

    public Guid CreatedBy { get; set; }
    public User? Creator { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? NewsId { get; set; }
    public News? News { get; set; }

    public int Attendees { get; set; } = 0;
}
