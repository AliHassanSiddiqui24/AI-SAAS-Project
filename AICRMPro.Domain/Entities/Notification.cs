namespace AICRMPro.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public User User { get; set; } = null!;
}

public enum NotificationType
{
    FollowUpDue,
    DealStale,
    AIInsight
}
