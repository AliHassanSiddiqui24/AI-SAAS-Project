namespace AICRMPro.Application.DTOs;

public class PagedResponse<T>
{
    public bool Success { get; set; }
    public List<T> Data { get; set; } = new List<T>();
    public Pagination Pagination { get; set; } = null!;
}

public class Pagination
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
