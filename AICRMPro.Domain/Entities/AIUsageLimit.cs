namespace AICRMPro.Domain.Entities;

public class AIUsageLimit : BaseEntity
{
    public Guid TenantId { get; set; }
    public DateOnly Date { get; set; }
    public int CallsUsed { get; set; }
    public int TokensUsed { get; set; }
    public decimal CostUSD { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
}
