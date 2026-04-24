namespace AICRMPro.Domain.Entities;

public class AIRequest : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public AIFeature Feature { get; set; }
    public AIProvider Provider { get; set; }
    public string Model { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    public decimal CostUSD { get; set; }
    public int LatencyMs { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public User User { get; set; } = null!;
}

public enum AIFeature
{
    EmailGen,
    LeadScore,
    ChatQuery,
    TagGen,
    Suggestion
}

public enum AIProvider
{
    Groq,
    OpenAI
}
