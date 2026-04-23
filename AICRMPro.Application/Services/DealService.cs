using Microsoft.EntityFrameworkCore;
using AICRMPro.Application.DTOs;
using AICRMPro.Domain.Entities;
using AICRMPro.Infrastructure.Data;

namespace AICRMPro.Application.Services;

public interface IDealService
{
    Task<PagedResponse<DealDto>> GetAllAsync(DealFilterDto filters);
    Task<DealDto?> GetByIdAsync(Guid id);
    Task<DealDto> CreateAsync(CreateDealDto dto);
    Task<DealDto> UpdateAsync(Guid id, UpdateDealDto dto);
    Task DeleteAsync(Guid id);
}

public class DealService : IDealService
{
    private readonly AppDbContext _context;

    public DealService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<DealDto>> GetAllAsync(DealFilterDto filters)
    {
        var query = _context.Deals
            .Include(d => d.Client)
            .AsQueryable();

        // Apply filters
        if (filters.Stage.HasValue)
        {
            query = query.Where(d => d.Stage == filters.Stage.Value);
        }

        if (filters.ClientId.HasValue)
        {
            query = query.Where(d => d.ClientId == filters.ClientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.ToLower();
            query = query.Where(d => 
                d.Title.ToLower().Contains(search) ||
                d.Client.Name.ToLower().Contains(search) ||
                d.Client.Company.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var deals = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(d => new DealDto
            {
                Id = d.Id,
                TenantId = d.TenantId,
                ClientId = d.ClientId,
                Title = d.Title,
                Value = d.Value,
                Stage = d.Stage,
                Probability = d.Probability,
                ExpectedCloseDate = d.ExpectedCloseDate,
                CreatedAt = d.CreatedAt,
                Client = new ClientDto
                {
                    Id = d.Client.Id,
                    TenantId = d.Client.TenantId,
                    Name = d.Client.Name,
                    Email = d.Client.Email,
                    Phone = d.Client.Phone,
                    Company = d.Client.Company,
                    Status = d.Client.Status,
                    LeadScore = d.Client.LeadScore,
                    Notes = d.Client.Notes,
                    CreatedAt = d.Client.CreatedAt
                }
            })
            .ToListAsync();

        return new PagedResponse<DealDto>
        {
            Success = true,
            Data = deals,
            Pagination = new Pagination
            {
                Page = filters.Page,
                PageSize = filters.PageSize,
                TotalCount = totalCount
            }
        };
    }

    public async Task<DealDto?> GetByIdAsync(Guid id)
    {
        var deal = await _context.Deals
            .Include(d => d.Client)
            .Where(d => d.Id == id)
            .Select(d => new DealDto
            {
                Id = d.Id,
                TenantId = d.TenantId,
                ClientId = d.ClientId,
                Title = d.Title,
                Value = d.Value,
                Stage = d.Stage,
                Probability = d.Probability,
                ExpectedCloseDate = d.ExpectedCloseDate,
                CreatedAt = d.CreatedAt,
                Client = new ClientDto
                {
                    Id = d.Client.Id,
                    TenantId = d.Client.TenantId,
                    Name = d.Client.Name,
                    Email = d.Client.Email,
                    Phone = d.Client.Phone,
                    Company = d.Client.Company,
                    Status = d.Client.Status,
                    LeadScore = d.Client.LeadScore,
                    Notes = d.Client.Notes,
                    CreatedAt = d.Client.CreatedAt
                }
            })
            .FirstOrDefaultAsync();

        return deal;
    }

    public async Task<DealDto> CreateAsync(CreateDealDto dto)
    {
        // Verify client exists
        var client = await _context.Clients.FindAsync(dto.ClientId);
        if (client == null)
        {
            throw new Exception("Client not found");
        }

        var deal = new Deal
        {
            ClientId = dto.ClientId,
            Title = dto.Title,
            Value = dto.Value,
            Stage = dto.Stage,
            Probability = dto.Probability,
            ExpectedCloseDate = dto.ExpectedCloseDate
        };

        _context.Deals.Add(deal);
        await _context.SaveChangesAsync();

        // Reload deal with client data
        var createdDeal = await _context.Deals
            .Include(d => d.Client)
            .FirstAsync(d => d.Id == deal.Id);

        return new DealDto
        {
            Id = createdDeal.Id,
            TenantId = createdDeal.TenantId,
            ClientId = createdDeal.ClientId,
            Title = createdDeal.Title,
            Value = createdDeal.Value,
            Stage = createdDeal.Stage,
            Probability = createdDeal.Probability,
            ExpectedCloseDate = createdDeal.ExpectedCloseDate,
            CreatedAt = createdDeal.CreatedAt,
            Client = new ClientDto
            {
                Id = createdDeal.Client.Id,
                TenantId = createdDeal.Client.TenantId,
                Name = createdDeal.Client.Name,
                Email = createdDeal.Client.Email,
                Phone = createdDeal.Client.Phone,
                Company = createdDeal.Client.Company,
                Status = createdDeal.Client.Status,
                LeadScore = createdDeal.Client.LeadScore,
                Notes = createdDeal.Client.Notes,
                CreatedAt = createdDeal.Client.CreatedAt
            }
        };
    }

    public async Task<DealDto> UpdateAsync(Guid id, UpdateDealDto dto)
    {
        var deal = await _context.Deals
            .Include(d => d.Client)
            .FirstOrDefaultAsync(d => d.Id == id);
        
        if (deal == null)
        {
            throw new Exception("Deal not found");
        }

        deal.Title = dto.Title;
        deal.Value = dto.Value;
        deal.Stage = dto.Stage;
        deal.Probability = dto.Probability;
        deal.ExpectedCloseDate = dto.ExpectedCloseDate;

        await _context.SaveChangesAsync();

        return new DealDto
        {
            Id = deal.Id,
            TenantId = deal.TenantId,
            ClientId = deal.ClientId,
            Title = deal.Title,
            Value = deal.Value,
            Stage = deal.Stage,
            Probability = deal.Probability,
            ExpectedCloseDate = deal.ExpectedCloseDate,
            CreatedAt = deal.CreatedAt,
            Client = new ClientDto
            {
                Id = deal.Client.Id,
                TenantId = deal.Client.TenantId,
                Name = deal.Client.Name,
                Email = deal.Client.Email,
                Phone = deal.Client.Phone,
                Company = deal.Client.Company,
                Status = deal.Client.Status,
                LeadScore = deal.Client.LeadScore,
                Notes = deal.Client.Notes,
                CreatedAt = deal.Client.CreatedAt
            }
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        var deal = await _context.Deals.FindAsync(id);
        if (deal == null)
        {
            throw new Exception("Deal not found");
        }

        _context.Deals.Remove(deal);
        await _context.SaveChangesAsync();
    }
}
