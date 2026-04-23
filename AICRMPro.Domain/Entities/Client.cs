namespace AICRMPro.Domain.Entities;

public class Client : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public ClientStatus Status { get; set; }
    public int LeadScore { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
}

public enum ClientStatus
{
    Hot,
    Warm,
    Cold
}
