-- Seed Data (sample)
INSERT INTO dbo.tbl_House (HouseId, AddressLine, City, [State], PostalCode, Latitude, Longitude)
VALUES
    (NEWID(), N'12 Residency Road', N'Chennai', N'Tamil Nadu', '600001', 13.0827, 80.2707),
    (NEWID(), N'18 MG Road', N'Bengaluru', N'Karnataka', '560001', 12.9716, 77.5946),
    (NEWID(), N'27 Marine Drive', N'Mumbai', N'Maharashtra', '400001', 19.0760, 72.8777);
GO
