USE [RealEstateMapDb]
GO

/****** Object:  Table [dbo].[tbl_State] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_State](
	[StateID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Code] [nvarchar](10) NULL,
PRIMARY KEY CLUSTERED ([StateID] ASC)
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[tbl_District] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_District](
	[DistrictID] [int] IDENTITY(1,1) NOT NULL,
	[StateID] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED ([DistrictID] ASC)
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[tbl_City] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_City](
	[CityID] [int] IDENTITY(1,1) NOT NULL,
	[DistrictID] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
PRIMARY KEY CLUSTERED ([CityID] ASC)
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[tbl_House] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_House](
	[HouseId] [uniqueidentifier] NOT NULL,
	[AddressLine] [nvarchar](256) NOT NULL,
	[City] [nvarchar](128) NOT NULL,
	[State] [nvarchar](128) NOT NULL,
	[PostalCode] [char](6) NOT NULL,
	[Latitude] [decimal](9, 6) NOT NULL,
	[Longitude] [decimal](9, 6) NOT NULL,
	[CreatedUtc] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	[Location] [geography] NULL,
PRIMARY KEY CLUSTERED ([HouseId] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- FKs
ALTER TABLE [dbo].[tbl_District] WITH CHECK ADD CONSTRAINT [FK_tbl_District_tbl_State] FOREIGN KEY([StateID])
REFERENCES [dbo].[tbl_State] ([StateID])
GO

ALTER TABLE [dbo].[tbl_City] WITH CHECK ADD CONSTRAINT [FK_tbl_City_tbl_District] FOREIGN KEY([DistrictID])
REFERENCES [dbo].[tbl_District] ([DistrictID])
GO
