using Microsoft.EntityFrameworkCore;
using AICRMPro.Application.DTOs;
using AICRMPro.Domain.Entities;
using AICRMPro.Infrastructure.Data;

namespace AICRMPro.Application.Services;

public interface IClientService
{
    Task<PagedResponse<ClientDto>> GetAllAsync(ClientFilterDto filters);
    Task<ClientDto?> GetByIdAsync(Guid id);
    Task<ClientDto> CreateAsync(CreateClientDto dto);
    Task<ClientDto> UpdateAsync(Guid id, UpdateClientDto dto);
    Task DeleteAsync(Guid id);
}

public class ClientService : IClientService
{
    private readonly AppDbContext _context;

    public ClientService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<ClientDto>> GetAllAsync(ClientFilterDto filters)
    {
        var query = _context.Clients.AsQueryable();

        // Apply filters
        if (filters.Status.HasValue)
        {
            query = query.Where(c => c.Status == filters.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.ToLower();
            query = query.Where(c => 
                c.Name.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search) ||
                c.Company.ToLower().Contains(search) ||
                c.Phone.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var clients = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                TenantId = c.TenantId,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Company = c.Company,
                Status = c.Status,
                LeadScore = c.LeadScore,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PagedResponse<ClientDto>
        {
            Success = true,
            Data = clients,
            Pagination = new Pagination
            {
                Page = filters.Page,
                PageSize = filters.PageSize,
                TotalCount = totalCount
            }
        };
    }

    public async Task<ClientDto?> GetByIdAsync(Guid id)
    {
        var client = await _context.Clients
            .Where(c => c.Id == id)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                TenantId = c.TenantId,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Company = c.Company,
                Status = c.Status,
                LeadScore = c.LeadScore,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();

        return client;
    }

    public async Task<ClientDto> CreateAsync(CreateClientDto dto)
    {
        var client = new Client
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Company = dto.Company,
            Status = dto.Status,
            LeadScore = dto.LeadScore,
            Notes = dto.Notes
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return new ClientDto
        {
            Id = client.Id,
            TenantId = client.TenantId,
            Name = client.Name,
            Email = client.Email,
            Phone = client.Phone,
            Company = client.Company,
            Status = client.Status,
            LeadScore = client.LeadScore,
            Notes = client.Notes,
            CreatedAt = client.CreatedAt
        };
    }

    public async Task<ClientDto> UpdateAsync(Guid id, UpdateClientDto dto)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null)
        {
            throw new Exception("Client not found");
        }

        client.Name = dto.Name;
        client.Email = dto.Email;
        client.Phone = dto.Phone;
        client.Company = dto.Company;
        client.Status = dto.Status;
        client.LeadScore = dto.LeadScore;
        client.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return new ClientDto
        {
            Id = client.Id,
            TenantId = client.TenantId,
            Name = client.Name,
            Email = client.Email,
            Phone = client.Phone,
            Company = client.Company,
            Status = client.Status,
            LeadScore = client.LeadScore,
            Notes = client.Notes,
            CreatedAt = client.CreatedAt
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null)
        {
            throw new Exception("Client not found");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    }
}

