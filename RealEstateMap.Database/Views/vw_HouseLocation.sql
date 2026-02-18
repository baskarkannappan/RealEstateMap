-- View: vw_HouseLocation
CREATE OR ALTER VIEW dbo.vw_HouseLocation
AS
SELECT
    HouseId,
    AddressLine,
    City,
    [State],
    PostalCode,
    Latitude,
    Longitude
FROM dbo.tbl_House;
GO
