using AICRMPro.Domain.Entities;
using AICRMPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace AICRMPro.Application.Services;

public class UsageLimitService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly AppDbContext _dbContext;
    private readonly ICurrentTenant _currentTenant;

    public UsageLimitService(IConnectionMultiplexer redis, AppDbContext dbContext, ICurrentTenant currentTenant)
    {
        _redis = redis;
        _dbContext = dbContext;
        _currentTenant = currentTenant;
    }

    public async Task<(bool IsAllowed, int CallsUsed)> CheckAndIncrementAsync(Guid tenantId)
    {
        var database = _redis.GetDatabase();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var redisKey = $"aillimit:{tenantId}:{today:yyyy-MM-dd}";

        // Get tenant's max AI calls per day
        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            throw new ArgumentException($"Tenant {tenantId} not found");
        }

        var maxCallsPerDay = tenant.MaxAICallsPerDay;

        // If unlimited (-1), just increment counter and allow
        if (maxCallsPerDay == -1)
        {
            var callsUsed = await database.StringIncrementAsync(redisKey);
            await EnsureTTLAsync(database, redisKey);
            await UpdateAIUsageLimitAsync(tenantId, today, (int)callsUsed, 0, 0);
            return (true, (int)callsUsed);
        }

        // Atomically increment the counter
        var callsUsedAtomic = await database.StringIncrementAsync(redisKey);

        // Check if limit exceeded
        if (callsUsedAtomic > maxCallsPerDay)
        {
            return (false, (int)callsUsedAtomic);
        }

        // Set TTL on first call (when TTL == -1)
        await EnsureTTLAsync(database, redisKey);

        // Update AIUsageLimits table
        await UpdateAIUsageLimitAsync(tenantId, today, (int)callsUsedAtomic, 0, 0);

        return (true, (int)callsUsedAtomic);
    }

    private async Task EnsureTTLAsync(IDatabase database, string redisKey)
    {
        var ttl = await database.KeyTimeToLiveAsync(redisKey);
        if (ttl == null || ttl.Value == TimeSpan.FromSeconds(-1)) // No TTL set
        {
            // Set TTL to seconds until midnight UTC
            var now = DateTime.UtcNow;
            var midnight = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Utc);
            var secondsUntilMidnight = (int)(midnight - now).TotalSeconds;
            
            if (secondsUntilMidnight > 0)
            {
                await database.KeyExpireAsync(redisKey, TimeSpan.FromSeconds(secondsUntilMidnight));
            }
        }
    }

    private async Task UpdateAIUsageLimitAsync(Guid tenantId, DateOnly date, int callsUsed, int tokensUsed, decimal costUsd)
    {
        try
        {
            var existingLimit = await _dbContext.AIUsageLimits
                .FirstOrDefaultAsync(ul => ul.TenantId == tenantId && ul.Date == date);

            if (existingLimit != null)
            {
                existingLimit.CallsUsed = callsUsed;
                existingLimit.TokensUsed = tokensUsed;
                existingLimit.CostUSD = costUsd;
            }
            else
            {
                var newLimit = new AIUsageLimit
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Date = date,
                    CallsUsed = callsUsed,
                    TokensUsed = tokensUsed,
                    CostUSD = costUsd,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.AIUsageLimits.Add(newLimit);
            }

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error but don't throw to avoid disrupting the main flow
            // In production, you'd want proper logging here
            Console.WriteLine($"Failed to update AI usage limit: {ex.Message}");
        }
    }

    public async Task UpdateTokenUsageAsync(Guid tenantId, int tokensUsed, decimal costUsd)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await UpdateAIUsageLimitAsync(tenantId, today, 0, tokensUsed, costUsd);
    }

    public async Task<int> GetCurrentUsageAsync(Guid tenantId)
    {
        var database = _redis.GetDatabase();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var redisKey = $"aillimit:{tenantId}:{today:yyyy-MM-dd}";

        var currentUsage = await database.StringGetAsync(redisKey);
        return currentUsage.HasValue ? (int)currentUsage : 0;
    }

    public async Task<(int CallsUsed, int TokensUsed, decimal CostUsd)> GetDetailedUsageAsync(Guid tenantId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var usageLimit = await _dbContext.AIUsageLimits
            .FirstOrDefaultAsync(ul => ul.TenantId == tenantId && ul.Date == today);

        if (usageLimit != null)
        {
            return (usageLimit.CallsUsed, usageLimit.TokensUsed, usageLimit.CostUSD);
        }

        return (0, 0, 0);
    }
}
