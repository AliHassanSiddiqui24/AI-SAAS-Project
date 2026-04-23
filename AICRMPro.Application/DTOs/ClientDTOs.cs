using AICRMPro.Domain.Entities;

namespace AICRMPro.Application.DTOs;

public class ClientDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public ClientStatus Status { get; set; }
    public int LeadScore { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateClientDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public ClientStatus Status { get; set; }
    public int LeadScore { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class UpdateClientDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public ClientStatus Status { get; set; }
    public int LeadScore { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class ClientFilterDto
{
    public ClientStatus? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
