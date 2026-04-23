namespace AICRMPro.Domain.Interfaces;

public interface ICurrentTenant
{
    Guid? TenantId { get; }
    void SetTenant(Guid? tenantId);
}
