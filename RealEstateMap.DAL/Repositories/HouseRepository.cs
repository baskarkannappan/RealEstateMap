using System.Data;
using RealEstateMap.DAL.Execution;
using RealEstateMap.Shared.Models;

namespace RealEstateMap.DAL.Repositories;

public sealed class HouseRepository : IHouseRepository
{
    private readonly IDbExecutor _dbExecutor;

    public HouseRepository(IDbExecutor dbExecutor)
    {
        _dbExecutor = dbExecutor;
    }

    public async Task<IReadOnlyList<HouseLocation>> SearchByBoundsAsync(
        double south,
        double west,
        double north,
        double east,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbExecutor.QueryAsync<HouseLocation>(
            "dbo.sp_House_SearchByBounds",
            new
            {
                South = south,
                West = west,
                North = north,
                East = east,
                Skip = pagination.Offset,
                Take = Math.Min(pagination.Take, 200) // Ensure request doesn't exceed 200
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return rows.ToList();
    }

    public async Task<IReadOnlyList<HouseLocation>> SearchByRadiusAsync(
        double centerLat,
        double centerLng,
        double radiusKm,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbExecutor.QueryAsync<HouseLocation>(
            "dbo.sp_House_SearchByRadius",
            new
            {
                CenterLat = centerLat,
                CenterLng = centerLng,
                RadiusKm = radiusKm,
                Skip = pagination.Offset,
                Take = Math.Min(pagination.Take, 200)
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return rows.ToList();
    }

    public async Task<IReadOnlyList<HouseLocation>> SearchByFiltersAsync(
        MapSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                HouseId,
                AddressLine AS [Address],
                City,
                [State],
                PostalCode,
                Latitude AS Lat,
                Longitude AS Lng,
                CAST(NULL AS float) AS DistanceKm
            FROM dbo.vw_HouseLocation
            WHERE (@PostalCode IS NULL OR PostalCode = @PostalCode)
              AND (@City IS NULL OR City = @City)
              AND (@State IS NULL OR [State] = @State)
            ORDER BY City, [Address]
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        var rows = await _dbExecutor.QueryAsync<HouseLocation>(
            sql,
            new
            {
                request.PostalCode,
                request.City,
                request.State,
                Skip = request.Pagination.Offset,
                Take = request.Pagination.Take
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        return rows.ToList();
    }
}
