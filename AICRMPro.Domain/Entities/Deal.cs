namespace AICRMPro.Domain.Entities;

public class Deal : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid ClientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DealStage Stage { get; set; }
    public int Probability { get; set; }
    public DateTime ExpectedCloseDate { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Client Client { get; set; } = null!;
}

public enum DealStage
{
    Lead,
    Qualified,
    Proposal,
    Negotiation,
    Won,
    Lost
}
