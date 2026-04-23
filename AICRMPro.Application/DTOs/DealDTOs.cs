using AICRMPro.Domain.Entities;

namespace AICRMPro.Application.DTOs;

public class DealDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ClientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DealStage Stage { get; set; }
    public int Probability { get; set; }
    public DateTime ExpectedCloseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public ClientDto? Client { get; set; }
}

public class CreateDealDto
{
    public Guid ClientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DealStage Stage { get; set; }
    public int Probability { get; set; }
    public DateTime ExpectedCloseDate { get; set; }
}

public class UpdateDealDto
{
    public string Title { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DealStage Stage { get; set; }
    public int Probability { get; set; }
    public DateTime ExpectedCloseDate { get; set; }
}

public class DealFilterDto
{
    public DealStage? Stage { get; set; }
    public Guid? ClientId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
