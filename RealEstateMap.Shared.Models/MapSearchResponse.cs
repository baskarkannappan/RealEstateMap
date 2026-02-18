namespace RealEstateMap.Shared.Models;

public sealed class MapSearchResponse
{
    public IReadOnlyList<HouseLocation> Houses { get; set; } = [];
    public PaginationResponse Pagination { get; set; } = new();
}
