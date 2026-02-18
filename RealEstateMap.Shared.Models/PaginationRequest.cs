namespace RealEstateMap.Shared.Models;

public sealed class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 250;

    public int Offset => Math.Max(0, (Math.Max(1, PageNumber) - 1) * Math.Clamp(PageSize, 1, 1000));
    public int Take => Math.Clamp(PageSize, 1, 1000);
}
