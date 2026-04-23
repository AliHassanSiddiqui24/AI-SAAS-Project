using AICRMPro.Domain.Interfaces;

namespace AICRMPro.Infrastructure.Services;

public class CurrentTenant : ICurrentTenant
{
    private Guid? _tenantId;

    public Guid? TenantId => _tenantId;

    public void SetTenant(Guid? tenantId)
    {
        _tenantId = tenantId;
    }
}
