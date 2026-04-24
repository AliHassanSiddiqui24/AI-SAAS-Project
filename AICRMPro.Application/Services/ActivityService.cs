using Microsoft.EntityFrameworkCore;
using AICRMPro.Application.DTOs;
using AICRMPro.Domain.Entities;
using AICRMPro.Infrastructure.Data;

namespace AICRMPro.Application.Services;

public interface IActivityService
{
    Task<PagedResponse<ActivityDto>> GetByClientIdAsync(Guid clientId, ActivityFilterDto filters);
    Task<ActivityDto> CreateAsync(CreateActivityDto dto);
    Task<ActivityDto> UpdateAsync(Guid id, UpdateActivityDto dto);
    Task<ActivityDto> CompleteAsync(Guid id, CompleteActivityDto dto);
}

public class ActivityService : IActivityService
{
    private readonly AppDbContext _context;

    public ActivityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<ActivityDto>> GetByClientIdAsync(Guid clientId, ActivityFilterDto filters)
    {
        var query = _context.Activities
            .Where(a => a.ClientId == clientId)
            .AsQueryable();

        // Apply type filter
        if (filters.Type.HasValue)
        {
            query = query.Where(a => a.Type == filters.Type.Value);
        }

        var totalCount = await query.CountAsync();

        var activities = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(a => new ActivityDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                ClientId = a.ClientId,
                DealId = a.DealId,
                UserId = a.UserId,
                Type = a.Type,
                Title = a.Title,
                Description = a.Description,
                Outcome = a.Outcome,
                ScheduledAt = a.ScheduledAt,
                CompletedAt = a.CompletedAt,
                IsCompleted = a.IsCompleted,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return new PagedResponse<ActivityDto>
        {
            Success = true,
            Data = activities,
            Pagination = new Pagination
            {
                Page = filters.Page,
                PageSize = filters.PageSize,
                TotalCount = totalCount
            }
        };
    }

    public async Task<ActivityDto> CreateAsync(CreateActivityDto dto)
    {
        var activity = new Activity
        {
            ClientId = dto.ClientId,
            DealId = dto.DealId,
            Type = dto.Type,
            Title = dto.Title,
            Description = dto.Description,
            ScheduledAt = dto.ScheduledAt,
            IsCompleted = false
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        return new ActivityDto
        {
            Id = activity.Id,
            TenantId = activity.TenantId,
            ClientId = activity.ClientId,
            DealId = activity.DealId,
            UserId = activity.UserId,
            Type = activity.Type,
            Title = activity.Title,
            Description = activity.Description,
            Outcome = activity.Outcome,
            ScheduledAt = activity.ScheduledAt,
            CompletedAt = activity.CompletedAt,
            IsCompleted = activity.IsCompleted,
            CreatedAt = activity.CreatedAt
        };
    }

    public async Task<ActivityDto> UpdateAsync(Guid id, UpdateActivityDto dto)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity == null)
        {
            throw new Exception("Activity not found");
        }

        activity.Type = dto.Type;
        activity.Title = dto.Title;
        activity.Description = dto.Description;
        activity.ScheduledAt = dto.ScheduledAt;

        await _context.SaveChangesAsync();

        return new ActivityDto
        {
            Id = activity.Id,
            TenantId = activity.TenantId,
            ClientId = activity.ClientId,
            DealId = activity.DealId,
            UserId = activity.UserId,
            Type = activity.Type,
            Title = activity.Title,
            Description = activity.Description,
            Outcome = activity.Outcome,
            ScheduledAt = activity.ScheduledAt,
            CompletedAt = activity.CompletedAt,
            IsCompleted = activity.IsCompleted,
            CreatedAt = activity.CreatedAt
        };
    }

    public async Task<ActivityDto> CompleteAsync(Guid id, CompleteActivityDto dto)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity == null)
        {
            throw new Exception("Activity not found");
        }

        activity.IsCompleted = true;
        activity.CompletedAt = DateTime.UtcNow;
        activity.Outcome = dto.Outcome;

        await _context.SaveChangesAsync();

        return new ActivityDto
        {
            Id = activity.Id,
            TenantId = activity.TenantId,
            ClientId = activity.ClientId,
            DealId = activity.DealId,
            UserId = activity.UserId,
            Type = activity.Type,
            Title = activity.Title,
            Description = activity.Description,
            Outcome = activity.Outcome,
            ScheduledAt = activity.ScheduledAt,
            CompletedAt = activity.CompletedAt,
            IsCompleted = activity.IsCompleted,
            CreatedAt = activity.CreatedAt
        };
    }
}
