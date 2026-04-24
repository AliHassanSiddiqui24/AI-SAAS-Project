using AICRMPro.Domain.Entities;

namespace AICRMPro.Application.DTOs;

public class ActivityDto
{
    public Guid Id { get; set; }
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
    public DateTime CreatedAt { get; set; }
}

public class CreateActivityDto
{
    public Guid ClientId { get; set; }
    public Guid? DealId { get; set; }
    public ActivityType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public class UpdateActivityDto
{
    public ActivityType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public class CompleteActivityDto
{
    public string? Outcome { get; set; }
}

public class ActivityFilterDto
{
    public ActivityType? Type { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
