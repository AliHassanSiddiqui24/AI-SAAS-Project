using System.IdentityModel.Tokens.Jwt;
using AICRMPro.Domain.Interfaces;

namespace AICRMPro.API.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenant currentTenant)
    {
        // Try to get tenant ID from JWT token
        var tenantId = ExtractTenantIdFromToken(context);

        // Set tenant in the current tenant service
        currentTenant.SetTenant(tenantId);

        await _next(context);
    }

    private Guid? ExtractTenantIdFromToken(HttpContext context)
    {
        try
        {
            // Get Authorization header
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            // Extract token
            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            // Read token without validation (just to extract claims)
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Extract tenantId claim
            var tenantIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "tenantId");
            if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                return null;
            }

            return tenantId;
        }
        catch
        {
            // If anything goes wrong, return null (no tenant context)
            return null;
        }
    }
}
