-- MS SQL Seed Data for All India Locations
-- Aligned with State -> District -> City Hierarchy

-- 1. States and Union Territories
IF NOT EXISTS (SELECT 1 FROM tbl_State WHERE Name = 'Tamil Nadu')
BEGIN
    INSERT INTO tbl_State (Name, Code) VALUES 
    ('Andaman and Nicobar Islands', 'AN'),
    ('Andhra Pradesh', 'AP'),
    ('Arunachal Pradesh', 'AR'),
    ('Assam', 'AS'),
    ('Bihar', 'BR'),
    ('Chandigarh', 'CH'),
    ('Chhattisgarh', 'CG'),
    ('Dadra and Nagar Haveli and Daman and Diu', 'DN'),
    ('Delhi', 'DL'),
    ('Goa', 'GA'),
    ('Gujarat', 'GJ'),
    ('Haryana', 'HR'),
    ('Himachal Pradesh', 'HP'),
    ('Jammu and Kashmir', 'JK'),
    ('Jharkhand', 'JH'),
    ('Karnataka', 'KA'),
    ('Kerala', 'KL'),
    ('Ladakh', 'LA'),
    ('Lakshadweep', 'LD'),
    ('Madhya Pradesh', 'MP'),
    ('Maharashtra', 'MH'),
    ('Manipur', 'MN'),
    ('Meghalaya', 'ML'),
    ('Mizoram', 'MZ'),
    ('Nagaland', 'NL'),
    ('Odisha', 'OR'),
    ('Puducherry', 'PY'),
    ('Punjab', 'PB'),
    ('Rajasthan', 'RJ'),
    ('Sikkim', 'SK'),
    ('Tamil Nadu', 'TN'),
    ('Telangana', 'TG'),
    ('Tripura', 'TR'),
    ('Uttar Pradesh', 'UP'),
    ('Uttarakhand', 'UK'),
    ('West Bengal', 'WB');
END

-- 2. Districts
DECLARE @StateID INT;

-- Tamil Nadu Districts
SET @StateID = (SELECT StateID FROM tbl_State WHERE Name = 'Tamil Nadu');
IF @StateID IS NOT NULL
BEGIN
    INSERT INTO tbl_District (StateID, Name)
    SELECT @StateID, d FROM (VALUES ('Chennai'), ('Coimbatore'), ('Madurai'), ('Vellore')) AS TN(d)
    WHERE NOT EXISTS (SELECT 1 FROM tbl_District WHERE StateID = @StateID AND Name = TN.d);
END

-- Karnataka Districts
SET @StateID = (SELECT StateID FROM tbl_State WHERE Name = 'Karnataka');
IF @StateID IS NOT NULL
BEGIN
    INSERT INTO tbl_District (StateID, Name)
    SELECT @StateID, d FROM (VALUES ('Bengaluru Urban'), ('Mysuru'), ('Dharwad')) AS KA(d)
    WHERE NOT EXISTS (SELECT 1 FROM tbl_District WHERE StateID = @StateID AND Name = KA.d);
END

-- Maharashtra Districts
SET @StateID = (SELECT StateID FROM tbl_State WHERE Name = 'Maharashtra');
IF @StateID IS NOT NULL
BEGIN
    INSERT INTO tbl_District (StateID, Name)
    SELECT @StateID, d FROM (VALUES ('Mumbai City'), ('Pune'), ('Nagpur')) AS MH(d)
    WHERE NOT EXISTS (SELECT 1 FROM tbl_District WHERE StateID = @StateID AND Name = MH.d);
END

-- 3. Cities
DECLARE @DistrictID INT;

-- Chennai Cities
SET @DistrictID = (SELECT DistrictID FROM tbl_District WHERE Name = 'Chennai');
IF @DistrictID IS NOT NULL
BEGIN
    INSERT INTO tbl_City (DistrictID, Name)
    SELECT @DistrictID, c FROM (VALUES ('Adyar'), ('Anna Nagar'), ('Besant Nagar'), ('Mylapore')) AS Chennai(c)
    WHERE NOT EXISTS (SELECT 1 FROM tbl_City WHERE DistrictID = @DistrictID AND Name = Chennai.c);
END

-- Bengaluru Cities
SET @DistrictID = (SELECT DistrictID FROM tbl_District WHERE Name = 'Bengaluru Urban');
IF @DistrictID IS NOT NULL
BEGIN
    INSERT INTO tbl_City (DistrictID, Name)
    SELECT @DistrictID, c FROM (VALUES ('Indiranagar'), ('Koramangala'), ('HSR Layout'), ('Whitefield')) AS Blr(c)
    WHERE NOT EXISTS (SELECT 1 FROM tbl_City WHERE DistrictID = @DistrictID AND Name = Blr.c);
END

-- Mumbai Cities
SET @DistrictID = (SELECT DistrictID FROM tbl_District WHERE Name = 'Mumbai City');
IF @DistrictID IS NOT NULL
BEGIN
    INSERT INTO tbl_City (DistrictID, Name)
    SELECT @DistrictID, c FROM (VALUES ('Bandra'), ('Andheri'), ('Colaba'), ('Dadar')) AS Mum(c)
    WHERE NOT EXISTS (SELECT 1 FROM tbl_City WHERE DistrictID = @DistrictID AND Name = Mum.c);
END
