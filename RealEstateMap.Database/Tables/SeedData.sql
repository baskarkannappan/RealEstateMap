-- Seed Data for Tamil Nadu Locations

DECLARE @StateID INT;

-- Insert State
INSERT INTO [dbo].[tbl_State] ([Name], [Code]) VALUES ('Tamil Nadu', 'TN');
SET @StateID = SCOPE_IDENTITY();

-- Insert Districts (Major Cities as Cities)
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Chennai');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Coimbatore');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Madurai');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Salem');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Tiruchirappalli');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Tiruppur');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Erode');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Vellore');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Thoothukudi');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Nagercoil');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Thanjavur');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Dindigul');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Kanchipuram');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Tiruvannamalai');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Kumbakonam');

-- Add more as needed based on the TamilNaduData.cs logic
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Adyar');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Anna Nagar');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Besant Nagar');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Mylapore');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'T. Nagar');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Velachery');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Tambaram');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Avadi');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Chromepet');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Polur');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Arani');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Chengalpattu');
INSERT INTO [dbo].[tbl_City] ([StateID], [Name]) VALUES (@StateID, 'Tambaram Central');
