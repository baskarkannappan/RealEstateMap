-- Table: tbl_House
CREATE TABLE dbo.tbl_House
(
    HouseId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    AddressLine NVARCHAR(256) NOT NULL,
    City NVARCHAR(128) NOT NULL,
    [State] NVARCHAR(128) NOT NULL,
    PostalCode CHAR(6) NOT NULL,
    Latitude FLOAT NOT NULL,
    Longitude FLOAT NOT NULL,
    CreatedUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE NONCLUSTERED INDEX IX_tbl_House_LatLng
ON dbo.tbl_House (Latitude, Longitude)
INCLUDE (HouseId, City, AddressLine);
CREATE INDEX IX_tbl_House_PostalCode ON dbo.tbl_House (PostalCode);
