-- ============================================================
-- CarShowroomDB - CLEAN DATABASE SCRIPT
-- Scope: Cars CRUD, Parts CRUD, Maintenance Packages/Services
-- Schema matches EF Core models exactly
-- ============================================================

USE [master]
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'CarShowroomDB')
    DROP DATABASE [CarShowroomDB]
GO

CREATE DATABASE [CarShowroomDB]
GO

USE [CarShowroomDB]
GO

-- ============================================================
-- 1. AppRoles
-- ============================================================
CREATE TABLE [dbo].[AppRoles] (
    [RoleId]    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [RoleName]  NVARCHAR(50) NOT NULL UNIQUE,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
)
GO

-- ============================================================
-- 2. AppUsers
-- ============================================================
CREATE TABLE [dbo].[AppUsers] (
    [UserId]           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FullName]         NVARCHAR(100) NOT NULL,
    [Email]            NVARCHAR(100) NOT NULL UNIQUE,
    [PasswordHash]     NVARCHAR(255) NOT NULL,
    [PhoneNumber]      NVARCHAR(20)  NULL,
    [Address]          NVARCHAR(255) NULL,
    [RoleId]           INT NOT NULL,
    [IsActive]         BIT NOT NULL DEFAULT 1,
    [VerificationCode] NVARCHAR(100) NULL,
    [CodeExpiryTime]   DATETIME NULL,
    [CreatedAt]        DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt]        DATETIME NULL,
    CONSTRAINT FK_AppUsers_AppRoles FOREIGN KEY ([RoleId]) REFERENCES [AppRoles]([RoleId])
)
GO

-- ============================================================
-- 3. CarBrands
-- ============================================================
CREATE TABLE [dbo].[CarBrands] (
    [BrandId]     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BrandName]   NVARCHAR(100) NOT NULL UNIQUE,
    [Country]     NVARCHAR(100) NULL,
    [Description] NVARCHAR(500) NULL,
    [LogoUrl]     NVARCHAR(500) NULL,
    [CreatedAt]   DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt]   DATETIME NULL
)
GO

-- ============================================================
-- 4. Cars
-- ============================================================
CREATE TABLE [dbo].[Cars] (
    [CarId]             INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BrandId]           INT NOT NULL,
    [CarName]           NVARCHAR(150) NOT NULL,
    [Model]             NVARCHAR(100) NULL,
    [Year]              INT NOT NULL,
    [Color]             NVARCHAR(50)  NULL,
    [Price]             DECIMAL(18,2) NOT NULL,
    [Mileage]           INT NOT NULL DEFAULT 0,
    [FuelType]          NVARCHAR(50)  NULL,
    [Transmission]      NVARCHAR(50)  NULL,
    [EngineCapacity]    DECIMAL(5,1)  NULL,
    [Status]            NVARCHAR(50)  NOT NULL DEFAULT 'Available',
    [ImageUrl]          NVARCHAR(500) NULL,
    [AdditionalImages]  NVARCHAR(MAX) NULL,
    [ReviewUrl]         NVARCHAR(500) NULL,
    [Description]       NVARCHAR(MAX) NULL,
    [CreatedAt]         DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt]         DATETIME NULL,
    [CreatedUser]       INT NULL,
    [UpdatedUser]       INT NULL,
    CONSTRAINT FK_Cars_CarBrands    FOREIGN KEY ([BrandId])     REFERENCES [CarBrands]([BrandId]),
    CONSTRAINT FK_Cars_CreatedUser  FOREIGN KEY ([CreatedUser]) REFERENCES [AppUsers]([UserId]),
    CONSTRAINT FK_Cars_UpdatedUser  FOREIGN KEY ([UpdatedUser]) REFERENCES [AppUsers]([UserId])
)
GO

-- ============================================================
-- 5. PartCategories
-- ============================================================
CREATE TABLE [dbo].[PartCategories] (
    [CategoryId]   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CategoryName] NVARCHAR(100) NOT NULL UNIQUE,
    [Description]  NVARCHAR(500) NULL,
    [CreatedAt]    DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt]    DATETIME NULL,
    [CreatedUser]  INT NULL,
    [UpdatedUser]  INT NULL,
    CONSTRAINT FK_PartCategories_CreatedUser FOREIGN KEY ([CreatedUser]) REFERENCES [AppUsers]([UserId]),
    CONSTRAINT FK_PartCategories_UpdatedUser FOREIGN KEY ([UpdatedUser]) REFERENCES [AppUsers]([UserId])
)
GO

-- ============================================================
-- 6. Parts  (matches model Part.cs)
-- ============================================================
CREATE TABLE [dbo].[Parts] (
    [PartId]            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CategoryId]        INT NOT NULL,
    [PartName]          NVARCHAR(150) NOT NULL,
    [PartCode]          NVARCHAR(50)  NOT NULL UNIQUE,
    [Brand]             NVARCHAR(100) NULL,
    [Price]             DECIMAL(18,2) NOT NULL,
    [Quantity]          INT NOT NULL DEFAULT 0,
    [MinStockLevel]     INT NOT NULL DEFAULT 5,
    [MaxStockLevel]     INT NOT NULL DEFAULT 100,
    [UnitOfMeasure]     NVARCHAR(50)  NOT NULL DEFAULT N'Cai',
    [WarehouseLocation] NVARCHAR(100) NULL,
    [WarrantyMonths]    INT NOT NULL DEFAULT 0,
    [Description]       NVARCHAR(MAX) NULL,
    [ImageUrl]          NVARCHAR(MAX) NULL,
    [Status]            NVARCHAR(50)  NOT NULL DEFAULT 'Available',
    [ExpiredAt]         DATETIME NULL,
    [CreatedAt]         DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt]         DATETIME NULL,
    [CreatedUser]       INT NULL,
    [UpdatedUser]       INT NULL,
    CONSTRAINT FK_Parts_PartCategories FOREIGN KEY ([CategoryId]) REFERENCES [PartCategories]([CategoryId]),
    CONSTRAINT FK_Parts_CreatedUser    FOREIGN KEY ([CreatedUser]) REFERENCES [AppUsers]([UserId]),
    CONSTRAINT FK_Parts_UpdatedUser    FOREIGN KEY ([UpdatedUser]) REFERENCES [AppUsers]([UserId])
)
GO

-- ============================================================
-- 7. Services  (matches model Service.cs: BasePrice, EstimatedDurationMinutes)
-- ============================================================
CREATE TABLE [dbo].[Services] (
    [ServiceId]                INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ServiceName]              NVARCHAR(150) NOT NULL,
    [Description]              NVARCHAR(MAX) NULL,
    [BasePrice]                DECIMAL(18,2) NOT NULL,
    [EstimatedDurationMinutes] INT NOT NULL DEFAULT 30,
    [Status]                   NVARCHAR(50) NOT NULL DEFAULT 'Available',
    [CreatedAt]                DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt]                DATETIME NULL,
    [CreatedUser]              INT NULL,
    [UpdatedUser]              INT NULL,
    CONSTRAINT FK_Services_CreatedUser FOREIGN KEY ([CreatedUser]) REFERENCES [AppUsers]([UserId]),
    CONSTRAINT FK_Services_UpdatedUser FOREIGN KEY ([UpdatedUser]) REFERENCES [AppUsers]([UserId])
)
GO

-- ============================================================
-- 8. MaintenancePackages  (matches model: PackagePrice instead of Price)
-- ============================================================
CREATE TABLE [dbo].[MaintenancePackages] (
    [PackageId]    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PackageName]  NVARCHAR(150) NOT NULL,
    [Description]  NVARCHAR(MAX) NULL,
    [PackagePrice] DECIMAL(18,2) NOT NULL,
    [Status]       NVARCHAR(50) NOT NULL DEFAULT 'Available',
    [CreatedAt]    DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt]    DATETIME NULL,
    [CreatedUser]  INT NULL,
    [UpdatedUser]  INT NULL,
    CONSTRAINT FK_MaintenancePackages_CreatedUser FOREIGN KEY ([CreatedUser]) REFERENCES [AppUsers]([UserId]),
    CONSTRAINT FK_MaintenancePackages_UpdatedUser FOREIGN KEY ([UpdatedUser]) REFERENCES [AppUsers]([UserId])
)
GO

-- ============================================================
-- 9. PackageServices  (matches model: added Notes column)
-- ============================================================
CREATE TABLE [dbo].[PackageServices] (
    [PackageServiceId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PackageId]        INT NOT NULL,
    [ServiceId]        INT NOT NULL,
    [Notes]            NVARCHAR(255) NULL,
    [CreatedAt]        DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_PackageServices_Packages  FOREIGN KEY ([PackageId]) REFERENCES [MaintenancePackages]([PackageId]) ON DELETE CASCADE,
    CONSTRAINT FK_PackageServices_Services  FOREIGN KEY ([ServiceId]) REFERENCES [Services]([ServiceId])
)
GO


-- ============================================================
-- SEED DATA
-- ============================================================

-- Roles
INSERT INTO [AppRoles] ([RoleName]) VALUES (N'Admin')
INSERT INTO [AppRoles] ([RoleName]) VALUES (N'Customer')
GO

-- Admin user  (password: Admin@123)
INSERT INTO [AppUsers] ([FullName],[Email],[PasswordHash],[PhoneNumber],[RoleId],[IsActive])
VALUES (N'Administrator', 'admin@dinhtoancar.vn',
        '$2a$11$k.3eFyB8FsXmSKdLXVoL7uHOL4A1EsJ9HuZD5F8Zt0M4k7PpZ6kky',
        '0901000001', 1, 1)
GO

-- Car Brands
INSERT INTO [CarBrands] ([BrandName],[Country]) VALUES (N'Toyota',        N'Japan')
INSERT INTO [CarBrands] ([BrandName],[Country]) VALUES (N'Honda',         N'Japan')
INSERT INTO [CarBrands] ([BrandName],[Country]) VALUES (N'Ford',          N'USA')
INSERT INTO [CarBrands] ([BrandName],[Country]) VALUES (N'VinFast',       N'Vietnam')
INSERT INTO [CarBrands] ([BrandName],[Country]) VALUES (N'Hyundai',       N'Korea')
INSERT INTO [CarBrands] ([BrandName],[Country]) VALUES (N'Mazda',         N'Japan')
INSERT INTO [CarBrands] ([BrandName],[Country]) VALUES (N'Kia',           N'Korea')
INSERT INTO [CarBrands] ([BrandName],[Country]) VALUES (N'Mercedes-Benz', N'Germany')
INSERT INTO [CarBrands] ([BrandName],[Country]) VALUES (N'BMW',           N'Germany')
INSERT INTO [CarBrands] ([BrandName],[Country]) VALUES (N'Audi',          N'Germany')
GO

-- Cars (3 xe mau)
INSERT INTO [Cars] ([BrandId],[CarName],[Model],[Year],[Color],[Price],[Mileage],[FuelType],[Transmission],[Status])
VALUES (1, N'Toyota Camry 2.5Q', N'Camry', 2022, N'Black', 1350000000, 0, N'Gasoline', N'Automatic', N'Available')

INSERT INTO [Cars] ([BrandId],[CarName],[Model],[Year],[Color],[Price],[Mileage],[FuelType],[Transmission],[Status])
VALUES (3, N'Ford Ranger Wildtrak 2.0L', N'Ranger', 2023, N'Orange', 960000000, 0, N'Diesel', N'Automatic', N'Available')

INSERT INTO [Cars] ([BrandId],[CarName],[Model],[Year],[Color],[Price],[Mileage],[FuelType],[Transmission],[Status])
VALUES (4, N'VinFast VF8 Plus', N'VF8', 2023, N'Blue', 1100000000, 0, N'Electric', N'Automatic', N'Available')
GO

-- Part Categories
INSERT INTO [PartCategories] ([CategoryName],[Description]) VALUES (N'Dong co & Truyen dong',  N'Cac bo phan lien quan den dong co, hop so va truyen dong')
INSERT INTO [PartCategories] ([CategoryName],[Description]) VALUES (N'He thong Dien & Ac quy', N'Ac quy, bong den, cau chi, ro le')
INSERT INTO [PartCategories] ([CategoryName],[Description]) VALUES (N'Dau nhot & Hoa chat',    N'Dau dong co, dau hop so, hoa chat lam sach')
INSERT INTO [PartCategories] ([CategoryName],[Description]) VALUES (N'Ngoai that & Phu kien',  N'Lop xe, gat mua, kinh chieu hau, den')
INSERT INTO [PartCategories] ([CategoryName],[Description]) VALUES (N'Phanh & He thong dung',  N'Ma phanh, dia phanh, dau phanh')
GO

-- Parts
INSERT INTO [Parts] ([CategoryId],[PartName],[PartCode],[Brand],[Price],[Quantity],[Status])
VALUES (4, N'Lop xe Michelin Pilot Sport 4', N'PT-LOP-001', N'Michelin', 3200000, 40, N'Available')

INSERT INTO [Parts] ([CategoryId],[PartName],[PartCode],[Brand],[Price],[Quantity],[Status])
VALUES (2, N'Ac quy GS 12V 45Ah', N'PT-ACQ-001', N'GS Battery', 1450000, 25, N'Available')

INSERT INTO [Parts] ([CategoryId],[PartName],[PartCode],[Brand],[Price],[Quantity],[Status])
VALUES (3, N'Dau nhot Castrol Magnatec 5W-30', N'PT-DAU-001', N'Castrol', 850000, 50, N'Available')

INSERT INTO [Parts] ([CategoryId],[PartName],[PartCode],[Brand],[Price],[Quantity],[Status])
VALUES (4, N'Gat mua Bosch Aerotwin', N'PT-BON-001', N'Bosch', 450000, 60, N'Available')

INSERT INTO [Parts] ([CategoryId],[PartName],[PartCode],[Brand],[Price],[Quantity],[Status])
VALUES (5, N'Ma phanh Brembo Front', N'PT-PHA-001', N'Brembo', 1200000, 15, N'Available')
GO

-- Services  (BasePrice + EstimatedDurationMinutes to match model)
INSERT INTO [Services] ([ServiceName],[Description],[BasePrice],[EstimatedDurationMinutes],[Status])
VALUES (N'Thay dau dong co', N'Thay dau va loc dau cho dong co', 350000, 30, N'Available')

INSERT INTO [Services] ([ServiceName],[Description],[BasePrice],[EstimatedDurationMinutes],[Status])
VALUES (N'Kiem tra tong quat xe', N'Kiem tra toan bo he thong xe', 200000, 60, N'Available')

INSERT INTO [Services] ([ServiceName],[Description],[BasePrice],[EstimatedDurationMinutes],[Status])
VALUES (N'Can bang & dao lop', N'Can bang va dao vi tri lop', 250000, 45, N'Available')

INSERT INTO [Services] ([ServiceName],[Description],[BasePrice],[EstimatedDurationMinutes],[Status])
VALUES (N'Ve sinh khoang may', N'Lam sach khoang dong co', 150000, 30, N'Available')

INSERT INTO [Services] ([ServiceName],[Description],[BasePrice],[EstimatedDurationMinutes],[Status])
VALUES (N'Kiem tra phanh', N'Kiem tra va dieu chinh he thong phanh', 180000, 30, N'Available')

INSERT INTO [Services] ([ServiceName],[Description],[BasePrice],[EstimatedDurationMinutes],[Status])
VALUES (N'Thay loc gio dieu hoa', N'Thay loc gio cabin dieu hoa', 120000, 20, N'Available')
GO

-- Maintenance Packages  (PackagePrice to match model)
INSERT INTO [MaintenancePackages] ([PackageName],[Description],[PackagePrice],[Status])
VALUES (N'Goi Bao Duong Co Ban', N'Danh cho xe moi hoac bao duong dinh ky co ban (5.000km - 10.000km)', 699000, N'Available')

INSERT INTO [MaintenancePackages] ([PackageName],[Description],[PackagePrice],[Status])
VALUES (N'Goi Bao Duong Tieu Chuan', N'Bao duong toan dien phu hop xe den 30.000km', 1290000, N'Available')

INSERT INTO [MaintenancePackages] ([PackageName],[Description],[PackagePrice],[Status])
VALUES (N'Goi Bao Duong Cao Cap', N'Goi day du nhat, kiem tra va bao duong toan bo he thong', 2190000, N'Available')
GO

-- Package Services
INSERT INTO [PackageServices] ([PackageId],[ServiceId]) VALUES (1, 1)
INSERT INTO [PackageServices] ([PackageId],[ServiceId]) VALUES (1, 2)

INSERT INTO [PackageServices] ([PackageId],[ServiceId]) VALUES (2, 1)
INSERT INTO [PackageServices] ([PackageId],[ServiceId]) VALUES (2, 2)
INSERT INTO [PackageServices] ([PackageId],[ServiceId]) VALUES (2, 3)
INSERT INTO [PackageServices] ([PackageId],[ServiceId]) VALUES (2, 4)

INSERT INTO [PackageServices] ([PackageId],[ServiceId]) VALUES (3, 1)
INSERT INTO [PackageServices] ([PackageId],[ServiceId]) VALUES (3, 2)
INSERT INTO [PackageServices] ([PackageId],[ServiceId]) VALUES (3, 3)
INSERT INTO [PackageServices] ([PackageId],[ServiceId]) VALUES (3, 4)
INSERT INTO [PackageServices] ([PackageId],[ServiceId]) VALUES (3, 5)
INSERT INTO [PackageServices] ([PackageId],[ServiceId]) VALUES (3, 6)
GO

PRINT N'Database CarShowroomDB created successfully!'
PRINT N'Tables: AppRoles, AppUsers, CarBrands, Cars, PartCategories, Parts, Services, MaintenancePackages, PackageServices'
GO
