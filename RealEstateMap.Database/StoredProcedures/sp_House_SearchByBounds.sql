-- Stored Procedure: sp_House_SearchByBounds
CREATE OR ALTER PROCEDURE dbo.sp_House_SearchByBounds
    @South FLOAT,
    @West FLOAT,
    @North FLOAT,
    @East FLOAT,
    @Skip INT = 0,
    @Take INT = 200
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        HouseId,
        AddressLine AS [Address],
        City,
        [State],
        PostalCode,
        Latitude AS Lat,
        Longitude AS Lng
    FROM dbo.tbl_House
    WHERE Latitude BETWEEN @South AND @North
      AND Longitude BETWEEN @West AND @East
    ORDER BY City, AddressLine
    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
END;
GO
