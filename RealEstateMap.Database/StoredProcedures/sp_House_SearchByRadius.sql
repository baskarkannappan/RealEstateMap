-- Stored Procedure: sp_House_SearchByRadius
CREATE OR ALTER PROCEDURE dbo.sp_House_SearchByRadius
    @CenterLat FLOAT,
    @CenterLng FLOAT,
    @RadiusKm FLOAT,
    @Skip INT = 0,
    @Take INT = 250
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Candidate AS
    (
        SELECT
            HouseId,
            AddressLine AS [Address],
            City,
            Latitude AS Lat,
            Longitude AS Lng,
            DistanceKm = (
                6371.0 * ACOS(
                    COS(RADIANS(@CenterLat)) * COS(RADIANS(Latitude)) *
                    COS(RADIANS(Longitude) - RADIANS(@CenterLng)) +
                    SIN(RADIANS(@CenterLat)) * SIN(RADIANS(Latitude))
                )
            )
        FROM dbo.tbl_House
    )
    SELECT HouseId, [Address], City, Lat, Lng, DistanceKm
    FROM Candidate
    WHERE DistanceKm <= @RadiusKm
    ORDER BY DistanceKm, City
    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
END;
GO
