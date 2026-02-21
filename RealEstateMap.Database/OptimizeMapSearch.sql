-- Database Optimization Script for Real Estate Map
SET QUOTED_IDENTIFIER ON;
SET ARITHABORT ON;
GO

-- Phase 3 — Spatial Column & Indexing
-- 1. Add spatial column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'Location' AND object_id = OBJECT_ID('dbo.tbl_House'))
BEGIN
    ALTER TABLE dbo.tbl_House ADD [Location] GEOGRAPHY;
END
GO

-- 2. Populate spatial column
UPDATE dbo.tbl_House
SET [Location] = GEOGRAPHY::Point(Latitude, Longitude, 4326)
WHERE [Location] IS NULL;
GO

-- 3. Create Spatial Index
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'SX_tbl_House_Location' AND object_id = OBJECT_ID('dbo.tbl_House'))
BEGIN
    CREATE SPATIAL INDEX SX_tbl_House_Location
    ON dbo.tbl_House([Location])
    USING GEOGRAPHY_AUTO_GRID;
END
GO

-- Phase 6 — Advanced Scaling
-- 4. Convert Latitude/Longitude Type from float to decimal(9,6)
-- Note: Must drop dependent indexes first.

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbl_House_Location' AND object_id = OBJECT_ID('dbo.tbl_House'))
BEGIN
    DROP INDEX IX_tbl_House_Location ON dbo.tbl_House;
END
GO

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbl_House_Coordinates' AND object_id = OBJECT_ID('dbo.tbl_House'))
BEGIN
    DROP INDEX IX_tbl_House_Coordinates ON dbo.tbl_House;
END
GO

ALTER TABLE dbo.tbl_House
ALTER COLUMN Latitude DECIMAL(9,6) NOT NULL;

ALTER TABLE dbo.tbl_House
ALTER COLUMN Longitude DECIMAL(9,6) NOT NULL;
GO

-- Recreate optimized non-clustered index on decimal columns
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbl_House_Coordinates' AND object_id = OBJECT_ID('dbo.tbl_House'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_tbl_House_Coordinates
    ON dbo.tbl_House (Latitude, Longitude)
    INCLUDE (HouseId, City, AddressLine);
END
GO

-- 5. Optimized Stored Procedure
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
        Latitude AS Lat,
        Longitude AS Lng
    FROM dbo.tbl_House
    WHERE Latitude BETWEEN @South AND @North
      AND Longitude BETWEEN @West AND @East
    ORDER BY City, AddressLine
    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
END;
GO
