namespace PortalForge.Domain.Entities;

public class ApprovalDelegation
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// User who is delegating their approval authority
    /// </summary>
    public Guid FromUserId { get; set; }
    public User FromUser { get; set; } = null!;
    
    /// <summary>
    /// User who will receive the delegated approval authority
    /// </summary>
    public Guid ToUserId { get; set; }
    public User ToUser { get; set; } = null!;
    
    /// <summary>
    /// When the delegation starts
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// When the delegation ends (null for indefinite)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Whether this delegation is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Reason for the delegation (e.g., vacation, sick leave)
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// When this delegation was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}