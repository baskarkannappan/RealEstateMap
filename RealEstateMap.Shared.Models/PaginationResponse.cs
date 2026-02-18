namespace RealEstateMap.Shared.Models;

public sealed class PaginationResponse
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int ReturnedCount { get; set; }
    public long? TotalCount { get; set; }
}
