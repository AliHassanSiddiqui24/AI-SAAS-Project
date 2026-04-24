namespace AICRMPro.Domain.Entities;

public class Activity : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid ClientId { get; set; }
    public Guid? DealId { get; set; }
    public Guid UserId { get; set; }
    public ActivityType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Outcome { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Client Client { get; set; } = null!;
    public Deal? Deal { get; set; }
    public User User { get; set; } = null!;
}

public enum ActivityType
{
    Call,
    Email,
    Meeting,
    Note,
    Task
}
