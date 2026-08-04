-- =============================================
-- CREATE DATABASE
-- =============================================

CREATE DATABASE CarShowroomDB;
GO

USE CarShowroomDB;
GO

-- =============================================
-- ROLES
-- =============================================

CREATE TABLE AppRoles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

-- =============================================
-- USERS
-- =============================================

CREATE TABLE AppUsers (
    UserId INT IDENTITY(1,1) PRIMARY KEY,

    FullName NVARCHAR(100) NOT NULL,

    Email NVARCHAR(100) NOT NULL UNIQUE,

    PasswordHash NVARCHAR(255) NOT NULL,

    PhoneNumber NVARCHAR(20),

    Address NVARCHAR(255),

    RoleId INT NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_AppUsers_AppRoles
        FOREIGN KEY(RoleId)
        REFERENCES AppRoles(RoleId)
);

-- =============================================
-- CAR BRANDS
-- =============================================

CREATE TABLE CarBrands (
    BrandId INT IDENTITY(1,1) PRIMARY KEY,

    BrandName NVARCHAR(100) NOT NULL,

    Country NVARCHAR(100),

    Description NVARCHAR(500)
);

-- =============================================
-- CARS
-- =============================================

CREATE TABLE Cars (
    CarId INT IDENTITY(1,1) PRIMARY KEY,

    BrandId INT NOT NULL,

    CarName NVARCHAR(150) NOT NULL,

    Model NVARCHAR(100),

    [Year] INT NOT NULL,

    Color NVARCHAR(50),

    Mileage INT NOT NULL DEFAULT 0,

    FuelType NVARCHAR(50) NOT NULL,

    Transmission NVARCHAR(50) NOT NULL,

    Price DECIMAL(18,2) NOT NULL,

    Description NVARCHAR(1000),

    ImageUrl NVARCHAR(500),

    Status NVARCHAR(50) NOT NULL
        DEFAULT 'Available'
        CHECK (Status IN
        ('Available','Reserved','Sold','Inactive')),

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Cars_CarBrands
        FOREIGN KEY (BrandId)
        REFERENCES CarBrands(BrandId)
);

-- =============================================
-- PURCHASE REQUESTS
-- =============================================

CREATE TABLE PurchaseRequests (
    RequestId INT IDENTITY(1,1) PRIMARY KEY,

    CarId INT NOT NULL,

    CustomerId INT NOT NULL,

    CustomerName NVARCHAR(100) NOT NULL,

    CustomerPhone NVARCHAR(20) NOT NULL,

    CustomerEmail NVARCHAR(100),

    Message NVARCHAR(1000),

    Status NVARCHAR(50) NOT NULL
        DEFAULT 'Pending'
        CHECK (Status IN
        ('Pending','Confirmed','Rejected','Completed')),

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME NULL,

    CONSTRAINT FK_PurchaseRequests_Cars
        FOREIGN KEY(CarId)
        REFERENCES Cars(CarId),

    CONSTRAINT FK_PurchaseRequests_AppUsers
        FOREIGN KEY(CustomerId)
        REFERENCES AppUsers(UserId)
);

-- =============================================
-- PART CATEGORIES
-- =============================================

CREATE TABLE PartCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,

    CategoryName NVARCHAR(100) NOT NULL,

    Description NVARCHAR(500)
);

-- =============================================
-- PARTS
-- =============================================

CREATE TABLE Parts (
    PartId INT IDENTITY(1,1) PRIMARY KEY,

    CategoryId INT NOT NULL,

    PartName NVARCHAR(150) NOT NULL,

    PartCode NVARCHAR(50) NOT NULL UNIQUE,

    Brand NVARCHAR(100),

    Price DECIMAL(18,2) NOT NULL,

    Quantity INT NOT NULL DEFAULT 0,

    Description NVARCHAR(1000),

    ImageUrl NVARCHAR(500),

    Status NVARCHAR(50) NOT NULL
        DEFAULT 'Available'
        CHECK (Status IN
        ('Available','OutOfStock','Inactive')),

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Parts_PartCategories
        FOREIGN KEY(CategoryId)
        REFERENCES PartCategories(CategoryId)
);

-- =============================================
-- PART ORDERS
-- =============================================

CREATE TABLE PartOrders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,

    CustomerId INT NOT NULL,

    CustomerName NVARCHAR(100) NOT NULL,

    CustomerPhone NVARCHAR(20) NOT NULL,

    CustomerEmail NVARCHAR(100),

    ShippingAddress NVARCHAR(255) NOT NULL,

    TotalAmount DECIMAL(18,2)
        NOT NULL DEFAULT 0,

    Status NVARCHAR(50) NOT NULL
        DEFAULT 'Pending'
        CHECK (Status IN
        ('Pending','Confirmed','Shipping',
         'Completed','Cancelled')),

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME NULL,

    CONSTRAINT FK_PartOrders_AppUsers
        FOREIGN KEY(CustomerId)
        REFERENCES AppUsers(UserId)
);

-- =============================================
-- PART ORDER DETAILS
-- =============================================

CREATE TABLE PartOrderDetails (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,

    OrderId INT NOT NULL,

    PartId INT NOT NULL,

    Quantity INT NOT NULL,

    UnitPrice DECIMAL(18,2) NOT NULL,

    SubTotal DECIMAL(18,2) NOT NULL,

    CONSTRAINT FK_PartOrderDetails_PartOrders
        FOREIGN KEY(OrderId)
        REFERENCES PartOrders(OrderId),

    CONSTRAINT FK_PartOrderDetails_Parts
        FOREIGN KEY(PartId)
        REFERENCES Parts(PartId)
);

-- =============================================
-- MAINTENANCE PACKAGES
-- =============================================

CREATE TABLE MaintenancePackages (
    PackageId INT IDENTITY(1,1) PRIMARY KEY,

    PackageName NVARCHAR(150) NOT NULL,

    Description NVARCHAR(1000),

    Price DECIMAL(18,2) NOT NULL,

    EstimatedDuration INT NOT NULL,

    Status NVARCHAR(50) NOT NULL
        DEFAULT 'Available'
        CHECK (Status IN
        ('Available','Inactive')),

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- =============================================
-- MAINTENANCE APPOINTMENTS
-- =============================================

CREATE TABLE MaintenanceAppointments (
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,

    CustomerId INT NOT NULL,

    PackageId INT NOT NULL,

    CustomerName NVARCHAR(100) NOT NULL,

    CustomerPhone NVARCHAR(20) NOT NULL,

    CustomerEmail NVARCHAR(100),

    CarName NVARCHAR(150) NOT NULL,

    LicensePlate NVARCHAR(30),

    AppointmentDate DATE NOT NULL,

    AppointmentTime TIME NOT NULL,

    Note NVARCHAR(1000),

    Status NVARCHAR(50) NOT NULL
        DEFAULT 'Pending'
        CHECK (Status IN
        ('Pending','Confirmed','InProgress',
         'Completed','Cancelled')),

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME NULL,

    CONSTRAINT FK_MaintenanceAppointments_AppUsers
        FOREIGN KEY(CustomerId)
        REFERENCES AppUsers(UserId),

    CONSTRAINT FK_MaintenanceAppointments_Packages
        FOREIGN KEY(PackageId)
        REFERENCES MaintenancePackages(PackageId)
);