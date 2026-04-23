using AICRMPro.Domain.Enums;

namespace AICRMPro.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Plan Plan { get; set; }
    public int MaxAICallsPerDay { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
}
