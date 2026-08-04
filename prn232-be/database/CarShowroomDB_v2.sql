-- =========================================================================
-- DATABASE: CarShowroomDB (Version 2.1 - Production Ready Audit Upgrade)
-- DESCRIPTION: High-performance, normalized, and robust database schema 
--              covering Car Sales, Spare Parts, Packages & Services, 
--              and a unified Master/Combined Invoice payment verification system.
--              Now upgraded with comprehensive audit logging (Created/Updated) 
--              and expiration tracking (ExpiredAt) fields.
-- =========================================================================

USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'CarShowroomDB')
BEGIN
    ALTER DATABASE CarShowroomDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CarShowroomDB;
END
GO

CREATE DATABASE CarShowroomDB;
GO

USE CarShowroomDB;
GO

-- =========================================================================
-- 1. SECURITY & USERS
-- =========================================================================

CREATE TABLE AppRoles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL
);

CREATE TABLE AppUsers (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Address NVARCHAR(255) NULL,
    RoleId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    VerificationCode NVARCHAR(100) NULL,
    CodeExpiryTime DATETIME NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_AppUsers_AppRoles FOREIGN KEY(RoleId) REFERENCES AppRoles(RoleId),
    CONSTRAINT FK_AppUsers_CreatedUser FOREIGN KEY(CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_AppUsers_UpdatedUser FOREIGN KEY(UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Self-referential constraints for AppRoles audit keys
ALTER TABLE AppRoles ADD CONSTRAINT FK_AppRoles_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId);
ALTER TABLE AppRoles ADD CONSTRAINT FK_AppRoles_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId);

-- =========================================================================
-- 2. CAR MANAGEMENT (Showroom Inventory)
-- =========================================================================

CREATE TABLE CarBrands (
    BrandId INT IDENTITY(1,1) PRIMARY KEY,
    BrandName NVARCHAR(100) NOT NULL,
    Country NVARCHAR(100) NULL,
    Description NVARCHAR(500) NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_CarBrands_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_CarBrands_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

CREATE TABLE Cars (
    CarId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    CarName NVARCHAR(150) NOT NULL,
    Model NVARCHAR(100) NULL,
    [Year] INT NOT NULL,
    Color NVARCHAR(50) NULL,
    Mileage INT NOT NULL DEFAULT 0,
    FuelType NVARCHAR(50) NOT NULL,
    Transmission NVARCHAR(50) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(1000) NULL,
    ImageUrl NVARCHAR(500) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Available'
        CONSTRAINT CK_Cars_Status CHECK (Status IN ('Available', 'Reserved', 'Sold', 'Inactive')),
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_Cars_CarBrands FOREIGN KEY (BrandId) REFERENCES CarBrands(BrandId),
    CONSTRAINT FK_Cars_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_Cars_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Customer registered cars (for service/maintenance logging)
CREATE TABLE CustomerCars (
    CustomerCarId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    BrandId INT NOT NULL,
    Model NVARCHAR(100) NOT NULL,
    [Year] INT NULL,
    VIN NVARCHAR(50) NULL UNIQUE, -- Vehicle Identification Number
    LicensePlate NVARCHAR(30) NOT NULL UNIQUE,
    Color NVARCHAR(50) NULL,
    
    -- Expiration field (E.g. Inspection/Registration validity)
    ExpiredAt DATETIME NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_CustomerCars_AppUsers FOREIGN KEY (CustomerId) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_CustomerCars_CarBrands FOREIGN KEY (BrandId) REFERENCES CarBrands(BrandId),
    CONSTRAINT FK_CustomerCars_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_CustomerCars_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- =========================================================================
-- 3. SPARE PARTS MODULE (Expanded Inventory & Logistics)
-- =========================================================================

CREATE TABLE Suppliers (
    SupplierId INT IDENTITY(1,1) PRIMARY KEY,
    SupplierName NVARCHAR(150) NOT NULL,
    ContactName NVARCHAR(100) NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    Address NVARCHAR(255) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active'
        CONSTRAINT CK_Suppliers_Status CHECK (Status IN ('Active', 'Inactive')),
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_Suppliers_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_Suppliers_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

CREATE TABLE PartCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_PartCategories_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_PartCategories_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

CREATE TABLE Parts (
    PartId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    PartName NVARCHAR(150) NOT NULL,
    PartCode VARCHAR(50) NOT NULL UNIQUE, -- E.g., OEM code or SKU
    Brand NVARCHAR(100) NULL,
    Price DECIMAL(18,2) NOT NULL,            -- Selling price to customer
    Quantity INT NOT NULL DEFAULT 0,          -- Current stock quantity
    MinStockLevel INT NOT NULL DEFAULT 5,     -- Reorder threshold point
    MaxStockLevel INT NOT NULL DEFAULT 100,   -- Maximum storage capacity
    UnitOfMeasure NVARCHAR(20) NOT NULL DEFAULT N'Cái', -- UoM: Cái, Bộ, Lít, Hộp...
    WarehouseLocation NVARCHAR(100) NULL,    -- E.g. Area A, Shelf 2, Bin C
    WarrantyMonths INT NOT NULL DEFAULT 0,
    Description NVARCHAR(1000) NULL,
    ImageUrl NVARCHAR(500) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Available'
        CONSTRAINT CK_Parts_Status CHECK (Status IN ('Available', 'OutOfStock', 'Inactive')),
    
    -- Expiration field (E.g. Shelf life / chemical expiration date)
    ExpiredAt DATETIME NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_Parts_PartCategories FOREIGN KEY(CategoryId) REFERENCES PartCategories(CategoryId),
    CONSTRAINT FK_Parts_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_Parts_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Fitting list/compatibilities to avoid customer ordering wrong parts
CREATE TABLE PartCompatibilities (
    CompatibilityId INT IDENTITY(1,1) PRIMARY KEY,
    PartId INT NOT NULL,
    BrandId INT NOT NULL,
    ModelName NVARCHAR(100) NOT NULL, -- Compatible model (e.g. Camry, Ranger)
    YearFrom INT NULL,
    YearTo INT NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_PartCompatibilities_Parts FOREIGN KEY (PartId) REFERENCES Parts(PartId) ON DELETE CASCADE,
    CONSTRAINT FK_PartCompatibilities_CarBrands FOREIGN KEY (BrandId) REFERENCES CarBrands(BrandId),
    CONSTRAINT FK_PartCompatibilities_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_PartCompatibilities_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Inventory purchase receipts from suppliers
CREATE TABLE InventoryReceipts (
    ReceiptId INT IDENTITY(1,1) PRIMARY KEY,
    SupplierId INT NOT NULL,
    StaffId INT NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ReceiptDate DATETIME NOT NULL DEFAULT GETDATE(),
    Notes NVARCHAR(1000) NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_InventoryReceipts_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(SupplierId),
    CONSTRAINT FK_InventoryReceipts_AppUsers FOREIGN KEY (StaffId) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_InventoryReceipts_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_InventoryReceipts_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

CREATE TABLE InventoryReceiptDetails (
    ReceiptDetailId INT IDENTITY(1,1) PRIMARY KEY,
    ReceiptId INT NOT NULL,
    PartId INT NOT NULL,
    Quantity INT NOT NULL,
    ImportPrice DECIMAL(18,2) NOT NULL,
    SubTotal AS (Quantity * ImportPrice),
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_InventoryReceiptDetails_Receipts FOREIGN KEY (ReceiptId) REFERENCES InventoryReceipts(ReceiptId) ON DELETE CASCADE,
    CONSTRAINT FK_InventoryReceiptDetails_Parts FOREIGN KEY (PartId) REFERENCES Parts(PartId),
    CONSTRAINT FK_InventoryReceiptDetails_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_InventoryReceiptDetails_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Comprehensive stock movement ledger (Audit log for inventory adjustments)
CREATE TABLE InventoryTransactions (
    TransactionId INT IDENTITY(1,1) PRIMARY KEY,
    PartId INT NOT NULL,
    TransactionType VARCHAR(20) NOT NULL
        CONSTRAINT CK_InventoryTransactions_Type CHECK (TransactionType IN ('Import', 'Export', 'Return', 'Adjustment')),
    Quantity INT NOT NULL, -- Positive for import/adjustment-in, negative for export
    ReferenceType VARCHAR(50) NULL, -- E.g. 'SupplierReceipt', 'PartOrder', 'ServiceUsage', 'StockCheck'
    ReferenceId INT NULL,          -- Links to ReceiptId, OrderId, AppointmentId, etc.
    StaffId INT NOT NULL,
    Notes NVARCHAR(500) NULL,
    TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_InventoryTransactions_Parts FOREIGN KEY (PartId) REFERENCES Parts(PartId),
    CONSTRAINT FK_InventoryTransactions_AppUsers FOREIGN KEY (StaffId) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_InventoryTransactions_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_InventoryTransactions_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- =========================================================================
-- 4. SERVICE & MAINTENANCE PACKAGE MODULE (Expanded Services)
-- =========================================================================

-- Individual core services (labor elements)
CREATE TABLE Services (
    ServiceId INT IDENTITY(1,1) PRIMARY KEY,
    ServiceName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(1000) NULL,
    BasePrice DECIMAL(18,2) NOT NULL, -- Labor cost / service fee
    EstimatedDurationMinutes INT NOT NULL DEFAULT 30,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Available'
        CONSTRAINT CK_Services_Status CHECK (Status IN ('Available', 'Inactive')),
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_Services_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_Services_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Composite packages (made up of multiple services)
CREATE TABLE MaintenancePackages (
    PackageId INT IDENTITY(1,1) PRIMARY KEY,
    PackageName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(1000) NULL,
    PackagePrice DECIMAL(18,2) NOT NULL, -- Custom discounted combo price
    Status NVARCHAR(50) NOT NULL DEFAULT 'Available'
        CONSTRAINT CK_MaintenancePackages_Status CHECK (Status IN ('Available', 'Inactive')),
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_MaintenancePackages_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_MaintenancePackages_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Relationship mapping services into packages
CREATE TABLE PackageServices (
    PackageId INT NOT NULL,
    ServiceId INT NOT NULL,
    Notes NVARCHAR(255) NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    PRIMARY KEY (PackageId, ServiceId),
    CONSTRAINT FK_PackageServices_Packages FOREIGN KEY (PackageId) REFERENCES MaintenancePackages(PackageId) ON DELETE CASCADE,
    CONSTRAINT FK_PackageServices_Services FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId),
    CONSTRAINT FK_PackageServices_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_PackageServices_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Relationship tracking parts consumed by default in a service (e.g. Oil change needs 4L oil)
CREATE TABLE ServiceRequiredParts (
    ServiceId INT NOT NULL,
    PartId INT NOT NULL,
    QuantityRequired INT NOT NULL DEFAULT 1,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    PRIMARY KEY (ServiceId, PartId),
    CONSTRAINT FK_ServiceRequiredParts_Services FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE CASCADE,
    CONSTRAINT FK_ServiceRequiredParts_Parts FOREIGN KEY (PartId) REFERENCES Parts(PartId),
    CONSTRAINT FK_ServiceRequiredParts_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_ServiceRequiredParts_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- =========================================================================
-- 5. MASTER INVOICING & PAYMENT GATEWAY (External Verification & Captcha)
-- =========================================================================

-- The Master Invoice holds general metadata, final sums, and payment status.
-- Both "Deposit" (Đặt cọc) and "Buyout" (Mua đứt) processes are verified here.
CREATE TABLE MasterInvoices (
    MasterInvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber VARCHAR(50) NOT NULL UNIQUE, -- E.g. INV-20260716-XXXX
    CustomerId INT NOT NULL,
    StaffId INT NULL,                       -- Employee confirming the external payment
    TotalSubTotal DECIMAL(18,2) NOT NULL DEFAULT 0, -- Combined sub-total of all modules
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0, -- Final amount (Subtotal - Discount + Tax)
    
    PurchaseType VARCHAR(20) NOT NULL DEFAULT 'Buyout'
        CONSTRAINT CK_MasterInvoices_PurchaseType CHECK (PurchaseType IN ('Deposit', 'Buyout')),
    
    PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'Unpaid'
        CONSTRAINT CK_MasterInvoices_PaymentStatus CHECK (PaymentStatus IN ('Unpaid', 'Deposited', 'Paid', 'PartiallyPaid', 'Refunded')),
        
    InvoiceStatus VARCHAR(20) NOT NULL DEFAULT 'Pending'
        CONSTRAINT CK_MasterInvoices_Status CHECK (InvoiceStatus IN ('Pending', 'PendingVerification', 'Confirmed', 'Completed', 'Cancelled')),
    
    -- --- Payment Stage 1: Deposit / Upfront Payment Verification ---
    DepositAmount DECIMAL(18,2) NULL,           -- Amount required to reserve (optional)
    DepositPaidAmount DECIMAL(18,2) NULL,       -- Actual amount paid for deposit
    DepositExpiresAt DATETIME NULL,             -- Expire reservation if captcha not entered
    DepositCaptchaCode VARCHAR(20) NULL,        -- Staff-generated code for customer verification
    IsDepositCaptchaUsed BIT NOT NULL DEFAULT 0,
    DepositCaptchaUsedAt DATETIME NULL,

    -- --- Payment Stage 2: Final Balance / Full Buyout Payment Verification ---
    FinalCaptchaCode VARCHAR(20) NULL,          -- Staff-generated code for buyout or balance payment
    IsFinalCaptchaUsed BIT NOT NULL DEFAULT 0,
    FinalCaptchaUsedAt DATETIME NULL,

    Notes NVARCHAR(1000) NULL,
    
    -- Expiration field (E.g. Invoice / Payment expiration date)
    ExpiredAt DATETIME NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_MasterInvoices_Customers FOREIGN KEY (CustomerId) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_MasterInvoices_Staff FOREIGN KEY (StaffId) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_MasterInvoices_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_MasterInvoices_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- =========================================================================
-- 6. INDIVIDUAL MODULE TRANSACTIONS (Specific Invoices)
-- =========================================================================

-- Operational: Customer Purchase Requests for a Car
CREATE TABLE PurchaseRequests (
    RequestId INT IDENTITY(1,1) PRIMARY KEY,
    CarId INT NOT NULL,
    CustomerId INT NOT NULL,
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerPhone NVARCHAR(20) NOT NULL,
    CustomerEmail NVARCHAR(100) NULL,
    Message NVARCHAR(1000) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending'
        CONSTRAINT CK_PurchaseRequests_Status CHECK (Status IN ('Pending', 'Confirmed', 'Rejected', 'Completed')),
    
    -- Expiration field (E.g. Reservation expiration date)
    ExpiredAt DATETIME NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_PurchaseRequests_Cars FOREIGN KEY(CarId) REFERENCES Cars(CarId),
    CONSTRAINT FK_PurchaseRequests_AppUsers FOREIGN KEY(CustomerId) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_PurchaseRequests_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_PurchaseRequests_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Operational: Spare Parts Orders
CREATE TABLE PartOrders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    MasterInvoiceId INT NULL,                -- Optional backlink to Master Invoice if combo
    CustomerId INT NOT NULL,
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerPhone NVARCHAR(20) NOT NULL,
    CustomerEmail NVARCHAR(100) NULL,
    ShippingAddress NVARCHAR(255) NULL,
    DeliveryMethod NVARCHAR(50) NOT NULL DEFAULT 'Pickup', -- 'Pickup' or 'Shipping'
    PaymentMethod NVARCHAR(50) NULL,
    ShippingFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending'
        CONSTRAINT CK_PartOrders_Status CHECK (Status IN ('Pending', 'Confirmed', 'Shipping', 'Completed', 'Cancelled')),
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_PartOrders_AppUsers FOREIGN KEY(CustomerId) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_PartOrders_MasterInvoices FOREIGN KEY(MasterInvoiceId) REFERENCES MasterInvoices(MasterInvoiceId) ON DELETE SET NULL,
    CONSTRAINT FK_PartOrders_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_PartOrders_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

CREATE TABLE PartOrderDetails (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    PartId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    SubTotal AS (Quantity * UnitPrice),
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_PartOrderDetails_PartOrders FOREIGN KEY(OrderId) REFERENCES PartOrders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_PartOrderDetails_Parts FOREIGN KEY(PartId) REFERENCES Parts(PartId),
    CONSTRAINT FK_PartOrderDetails_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_PartOrderDetails_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Operational: Maintenance & Service Appointments
CREATE TABLE MaintenanceAppointments (
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
    MasterInvoiceId INT NULL,                -- Optional backlink if invoiced
    CustomerId INT NOT NULL,
    CustomerCarId INT NOT NULL,             -- Relies on registered customer car
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerPhone NVARCHAR(20) NOT NULL,
    CustomerEmail NVARCHAR(100) NULL,
    AppointmentDate DATE NOT NULL,
    AppointmentTime TIME NOT NULL,
    Note NVARCHAR(1000) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending'
        CONSTRAINT CK_MaintenanceAppointments_Status CHECK (Status IN ('Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled')),
    
    -- Expiration field (E.g. Appointment check-in timeout)
    ExpiredAt DATETIME NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_MaintenanceAppointments_AppUsers FOREIGN KEY(CustomerId) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_MaintenanceAppointments_CustomerCars FOREIGN KEY(CustomerCarId) REFERENCES CustomerCars(CustomerCarId),
    CONSTRAINT FK_MaintenanceAppointments_MasterInvoices FOREIGN KEY(MasterInvoiceId) REFERENCES MasterInvoices(MasterInvoiceId) ON DELETE SET NULL,
    CONSTRAINT FK_MaintenanceAppointments_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_MaintenanceAppointments_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Detailing what packages and/or services are booked in an appointment
CREATE TABLE AppointmentDetails (
    AppointmentDetailId INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    PackageId INT NULL,                     -- Null if they book individual services
    ServiceId INT NULL,                     -- Null if they book a package
    UnitPrice DECIMAL(18,2) NOT NULL,       -- Locked price at booking
    Quantity INT NOT NULL DEFAULT 1,
    SubTotal AS (Quantity * UnitPrice),
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_AppointmentDetails_Appointments FOREIGN KEY(AppointmentId) REFERENCES MaintenanceAppointments(AppointmentId) ON DELETE CASCADE,
    CONSTRAINT FK_AppointmentDetails_Packages FOREIGN KEY(PackageId) REFERENCES MaintenancePackages(PackageId),
    CONSTRAINT FK_AppointmentDetails_Services FOREIGN KEY(ServiceId) REFERENCES Services(ServiceId),
    CONSTRAINT FK_AppointmentDetails_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_AppointmentDetails_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT CK_Detail_Reference CHECK (
        (PackageId IS NOT NULL AND ServiceId IS NULL) OR
        (PackageId IS NULL AND ServiceId IS NOT NULL)
    )
);

-- Track parts consumed during an appointment (both standard and unplanned/incurred)
CREATE TABLE AppointmentConsumedParts (
    ConsumedPartId INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    AppointmentDetailId INT NULL,            -- Associated service detail if applicable
    PartId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,       -- Selling price locked at time of usage
    IsIncurred BIT NOT NULL DEFAULT 0,       -- 0 = Standard required part, 1 = Incurred/additional part (Phụ tùng phát sinh)
    ApprovedByCustomer BIT NOT NULL DEFAULT 1, -- Auto-approved for standard parts, needs customer approval for incurred parts
    Notes NVARCHAR(500) NULL,                -- E.g., Reason for incurred parts
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_AppointmentConsumedParts_Appointments FOREIGN KEY(AppointmentId) REFERENCES MaintenanceAppointments(AppointmentId) ON DELETE CASCADE,
    CONSTRAINT FK_AppointmentConsumedParts_Details FOREIGN KEY(AppointmentDetailId) REFERENCES AppointmentDetails(AppointmentDetailId),
    CONSTRAINT FK_AppointmentConsumedParts_Parts FOREIGN KEY(PartId) REFERENCES Parts(PartId),
    CONSTRAINT FK_AppointmentConsumedParts_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_AppointmentConsumedParts_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Staff assignments per service in appointment
CREATE TABLE ServiceStaffAssignments (
    AssignmentId INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    ServiceId INT NOT NULL,
    StaffId INT NOT NULL,                    -- Assigned mechanic / technician
    AssignedAt DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Assigned'
        CONSTRAINT CK_ServiceStaffAssignments_Status CHECK (Status IN ('Assigned', 'InProgress', 'Completed', 'Reassigned')),
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_ServiceStaffAssignments_Appointments FOREIGN KEY (AppointmentId) REFERENCES MaintenanceAppointments(AppointmentId) ON DELETE CASCADE,
    CONSTRAINT FK_ServiceStaffAssignments_Services FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId),
    CONSTRAINT FK_ServiceStaffAssignments_Staff FOREIGN KEY (StaffId) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_ServiceStaffAssignments_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_ServiceStaffAssignments_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- Progress logs for service execution
CREATE TABLE ServiceExecutionLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentDetailId INT NOT NULL,
    StaffId INT NOT NULL,
    LogStatus NVARCHAR(50) NOT NULL DEFAULT 'Started'
        CONSTRAINT CK_ServiceExecutionLogs_Status CHECK (LogStatus IN ('Started', 'Completed', 'Blocked')),
    Notes NVARCHAR(1000) NULL,
    RecordedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_ServiceExecutionLogs_Details FOREIGN KEY (AppointmentDetailId) REFERENCES AppointmentDetails(AppointmentDetailId) ON DELETE CASCADE,
    CONSTRAINT FK_ServiceExecutionLogs_Staff FOREIGN KEY (StaffId) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_ServiceExecutionLogs_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_ServiceExecutionLogs_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- -------------------------------------------------------------------------
-- Financials: SPECIFIC INVOICES (Mapped 1-1 or N-1 to Master Invoice)
-- -------------------------------------------------------------------------

-- A: CAR INVOICE
CREATE TABLE CarInvoices (
    CarInvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    MasterInvoiceId INT NOT NULL,
    CarId INT NOT NULL,
    PurchaseRequestId INT NULL, -- Backreference if generated from request
    UnitPrice DECIMAL(18,2) NOT NULL,
    RegistrationFee DECIMAL(18,2) NOT NULL DEFAULT 0, -- Tax/Fee e.g. lệ phí trước bạ
    PlateFee DECIMAL(18,2) NOT NULL DEFAULT 0,        -- Plate registration fee
    InsuranceFee DECIMAL(18,2) NOT NULL DEFAULT 0,    -- Civil liability / physical insurance
    SubTotal AS (UnitPrice + RegistrationFee + PlateFee + InsuranceFee),
    Notes NVARCHAR(500) NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_CarInvoices_MasterInvoices FOREIGN KEY (MasterInvoiceId) REFERENCES MasterInvoices(MasterInvoiceId) ON DELETE CASCADE,
    CONSTRAINT FK_CarInvoices_Cars FOREIGN KEY (CarId) REFERENCES Cars(CarId),
    CONSTRAINT FK_CarInvoices_PurchaseRequests FOREIGN KEY (PurchaseRequestId) REFERENCES PurchaseRequests(RequestId),
    CONSTRAINT FK_CarInvoices_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_CarInvoices_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- B: SPARE PARTS INVOICE
CREATE TABLE PartInvoices (
    PartInvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    MasterInvoiceId INT NOT NULL,
    PartOrderId INT NOT NULL UNIQUE, -- One invoice per operational parts order
    SubTotal DECIMAL(18,2) NOT NULL,
    ShippingFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount AS (SubTotal + ShippingFee + TaxAmount),
    Notes NVARCHAR(500) NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_PartInvoices_MasterInvoices FOREIGN KEY (MasterInvoiceId) REFERENCES MasterInvoices(MasterInvoiceId) ON DELETE CASCADE,
    CONSTRAINT FK_PartInvoices_PartOrders FOREIGN KEY (PartOrderId) REFERENCES PartOrders(OrderId),
    CONSTRAINT FK_PartInvoices_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_PartInvoices_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- C: SERVICE INVOICE (Generated upon completion or prepay)
CREATE TABLE ServiceInvoices (
    ServiceInvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    MasterInvoiceId INT NOT NULL,
    AppointmentId INT NOT NULL UNIQUE, -- One invoice per appointment session
    SubTotal DECIMAL(18,2) NOT NULL,    -- Sum of labor (services) + parts consumed
    LaborDiscount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount AS (SubTotal - LaborDiscount),
    Notes NVARCHAR(500) NULL,
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_ServiceInvoices_MasterInvoices FOREIGN KEY (MasterInvoiceId) REFERENCES MasterInvoices(MasterInvoiceId) ON DELETE CASCADE,
    CONSTRAINT FK_ServiceInvoices_Appointments FOREIGN KEY (AppointmentId) REFERENCES MaintenanceAppointments(AppointmentId),
    CONSTRAINT FK_ServiceInvoices_CreatedUser FOREIGN KEY (CreatedUser) REFERENCES AppUsers(UserId),
    CONSTRAINT FK_ServiceInvoices_UpdatedUser FOREIGN KEY (UpdatedUser) REFERENCES AppUsers(UserId)
);

-- =========================================================================
-- INDEXES FOR PERFORMANCE OPTIMIZATION
-- =========================================================================
CREATE INDEX IX_Cars_BrandId ON Cars(BrandId);
CREATE INDEX IX_Parts_CategoryId ON Parts(CategoryId);
CREATE INDEX IX_Parts_PartCode ON Parts(PartCode);
CREATE INDEX IX_CustomerCars_CustomerId ON CustomerCars(CustomerId);
CREATE INDEX IX_MaintenanceAppointments_CustomerId ON MaintenanceAppointments(CustomerId);
CREATE INDEX IX_PartCompatibilities_PartId ON PartCompatibilities(PartId);
CREATE INDEX IX_AppointmentConsumedParts_AppointmentId ON AppointmentConsumedParts(AppointmentId);
CREATE INDEX IX_MasterInvoices_CustomerId ON MasterInvoices(CustomerId);
CREATE INDEX IX_MasterInvoices_InvoiceNumber ON MasterInvoices(InvoiceNumber);
CREATE INDEX IX_CarInvoices_MasterInvoiceId ON CarInvoices(MasterInvoiceId);
CREATE INDEX IX_PartInvoices_MasterInvoiceId ON PartInvoices(MasterInvoiceId);
CREATE INDEX IX_ServiceInvoices_MasterInvoiceId ON ServiceInvoices(MasterInvoiceId);

-- =========================================================================
-- SEED DATA FOR DEMO & TESTING
-- =========================================================================

-- 1. AppRoles Seed
INSERT INTO AppRoles (RoleName) VALUES ('Admin'), ('Staff'), ('Customer');

-- 2. AppUsers Seed
-- Passwords are set to BCrypt hashes or standard hashes. 
-- 'admin@gmail.com' (Admin, pass: admin), 'staff@gmail.com' (Staff, pass: staff), 'customer@gmail.com' (Customer, pass: customer)
INSERT INTO AppUsers (FullName, Email, PasswordHash, PhoneNumber, Address, RoleId, IsActive)
VALUES 
(N'Quản trị viên Hệ thống', 'admin@gmail.com', '$2a$11$ivuFcskipHfVJyUk7X7Cy.72DYWJAKQhFt7uaF2kMrwZ/LAHW1cWO', '0987654321', N'Hà Nội', 1, 1),
(N'Nguyễn Văn Nhân Viên', 'staff@gmail.com', '$2a$11$ivuFcskipHfVJyUk7X7Cy.72DYWJAKQhFt7uaF2kMrwZ/LAHW1cWO', '0912345678', N'Đà Nẵng', 2, 1),
(N'Trần Văn Khách Hàng', 'customer@gmail.com', '$2a$11$iR0JU.l1mLeRCyKuClJFxuWqtweaw2kS3oZSRG/lAcD00M603P5Mm', '0123456789', N'TP. Hồ Chí Minh', 3, 1);

-- Set admin user as the creator of the seeded roles and users
UPDATE AppRoles SET CreatedUser = 1;
UPDATE AppUsers SET CreatedUser = 1 WHERE UserId IN (1,2,3);

-- 3. CarBrands Seed
INSERT INTO CarBrands (BrandName, Country, Description, CreatedUser)
VALUES 
('Toyota', 'Japan', 'Toyota Motor Corporation', 1),
('Ford', 'USA', 'Ford Motor Company', 1),
('VinFast', 'Vietnam', 'VinFast Vietnam', 1),
('BMW', 'Germany', 'Bayerische Motoren Werke AG', 1);

-- 4. Cars Seed
INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedUser)
VALUES 
(1, 'Toyota Camry 2.5Q', 'Camry', 2022, 'Black', 15000, 'Gasoline', 'Automatic', 1350000000.00, N'Xe sang trọng, gia đình sử dụng kỹ, bảo dưỡng chính hãng.', 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?auto=format&fit=crop&w=600&q=80', 'Available', 1),
(2, 'Ford Ranger Wildtrak 2.0L', 'Ranger', 2023, 'Orange', 8000, 'Diesel', 'Automatic', 960000000.00, N'Vua bán tải, phiên bản cao cấp nhất Wildtrak 2 cầu, đầy đủ công nghệ.', 'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?auto=format&fit=crop&w=600&q=80', 'Available', 1),
(3, 'VinFast VF8 Plus', 'VF8', 2023, 'Blue', 5000, 'Electric', 'Automatic', 1100000000.00, N'Xe điện thông minh Việt Nam, bản Plus pin SDI, công nghệ ADAS hiện đại.', 'https://images.unsplash.com/photo-1563720223185-11003d516935?auto=format&fit=crop&w=600&q=80', 'Available', 1);

-- 5. CustomerCars Seed
INSERT INTO CustomerCars (CustomerId, BrandId, Model, [Year], VIN, LicensePlate, Color, ExpiredAt, CreatedUser)
VALUES 
(3, 1, 'Toyota Vios 1.5G', 2021, 'VIN123456789ABCDEF', '30G-888.88', 'White', '2027-01-01 00:00:00', 1);

-- 6. Suppliers Seed
INSERT INTO Suppliers (SupplierName, ContactName, Phone, Email, Address, Status, CreatedUser)
VALUES 
(N'Công Ty Phụ Tùng Ô Tô Bosch Việt Nam', N'Trần Minh Đức', '0243999888', 'contact@bosch.com.vn', N'Quận 1, TP. HCM', 'Active', 1),
(N'Tổng Kho Phụ Tùng Michelin Hà Nội', N'Lê Hoàng Nam', '0243666777', 'sales@michelin.vn', N'Long Biên, Hà Nội', 'Active', 1);

-- 7. PartCategories Seed
INSERT INTO PartCategories (CategoryName, Description, CreatedUser)
VALUES 
(N'Động cơ & Truyền động', N'Các bộ phận liên quan đến động cơ, hộp số và truyền động.', 1),
(N'Hệ thống điện & Ắc quy', N'Ắc quy, máy phát điện, đèn và hệ thống điện.', 1),
(N'Dầu nhớt & Hóa chất', N'Dầu máy, nước làm mát, dầu phanh và hóa chất bảo dưỡng.', 1),
(N'Ngoại thất & Phụ kiện', N'Lốp xe, gạt mưa, gương và các phụ kiện trang trí ngoại thất.', 1);

-- 8. Parts Seed
-- Parts seeded with ExpiredAt (shelf life for engine oils or batteries)
INSERT INTO Parts (CategoryId, PartName, PartCode, Brand, Price, Quantity, MinStockLevel, MaxStockLevel, UnitOfMeasure, WarehouseLocation, WarrantyMonths, Status, ExpiredAt, CreatedUser)
VALUES 
(4, N'Lốp xe Michelin Pilot Sport 4', 'PT-MIC-PS4', 'Michelin', 3200000.00, 40, 10, 100, N'Cái', 'Khu A - Kệ 3 - Ngăn 1', 12, 'Available', '2030-12-31', 1),
(2, N'Ắc quy GS 12V 45Ah', 'PT-GS-12V45', 'GS Battery', 1450000.00, 25, 5, 50, N'Cái', 'Khu B - Kệ 1 - Ngăn 2', 9, 'Available', '2028-06-30', 1),
(3, N'Dầu nhớt Castrol Magnatec 5W-30', 'PT-CAS-5W30', 'Castrol', 850000.00, 50, 15, 200, N'Lít', 'Khu C - Hàng 2', 0, 'Available', '2029-05-15', 1),
(4, N'Gạt mưa Bosch Aerotwin', 'PT-BOS-AERO', 'Bosch', 450000.00, 60, 10, 150, N'Cặp', 'Khu A - Kệ 1 - Ngăn 4', 6, 'Available', '2031-01-01', 1);

-- 9. PartCompatibilities Seed
INSERT INTO PartCompatibilities (PartId, BrandId, ModelName, YearFrom, YearTo, CreatedUser)
VALUES 
(1, 1, 'Camry', 2018, 2024, 1),
(1, 1, 'Altis', 2019, 2024, 1),
(4, 1, 'Vios', 2017, 2023, 1),
(4, 2, 'Ranger', 2018, 2023, 1);

-- 10. Services Seed
INSERT INTO Services (ServiceName, Description, BasePrice, EstimatedDurationMinutes, Status, CreatedUser)
VALUES 
(N'Thay dầu động cơ & Cốc lọc dầu', N'Xả dầu cũ, thay lọc dầu chính hãng, châm dầu động cơ Castrol mới phù hợp.', 200000.00, 30, 'Available', 1),
(N'Cân chỉnh thước lái độ chụm lốp', N'Sử dụng máy quét laser 3D để căn chỉnh độ chụm bánh xe và cân bằng động.', 450000.00, 45, 'Available', 1),
(N'Vệ sinh dàn lạnh điều hòa nội thất', N'Sử dụng máy nội soi chuyên dụng làm sạch bụi bẩn dàn lạnh không cần tháo taplo.', 600000.00, 60, 'Available', 1),
(N'Kiểm tra toàn diện 30 hạng mục kỹ thuật', N'Kiểm tra máy gầm, phanh, lốp, điện thân xe, nước làm mát, chẩn đoán lỗi bằng máy chuyên dụng.', 150000.00, 40, 'Available', 1);

-- 11. MaintenancePackages Seed
INSERT INTO MaintenancePackages (PackageName, Description, PackagePrice, Status, CreatedUser)
VALUES 
(N'Bảo dưỡng Định kỳ Tiêu chuẩn 10.000km', N'Gói bảo dưỡng cơ bản giúp xe vận hành trơn tru bao gồm thay dầu, kiểm tra phanh và rà soát lỗi.', 1200000.00, 'Available', 1),
(N'Chăm sóc Điều hòa VIP đón hè', N'Làm lạnh sâu, diệt khuẩn dàn lạnh điều hòa nội thất.', 950000.00, 'Available', 1);

-- 12. PackageServices Junction Seed
INSERT INTO PackageServices (PackageId, ServiceId, CreatedUser)
VALUES 
(1, 1, 1), -- Gói 1 có dịch vụ thay dầu
(1, 4, 1), -- Gói 1 có dịch vụ kiểm tra 30 hạng mục
(2, 3, 1), -- Gói 2 có dịch vụ vệ sinh dàn lạnh
(2, 4, 1); -- Gói 2 có dịch vụ kiểm tra 30 hạng mục

-- 13. ServiceRequiredParts Seed
INSERT INTO ServiceRequiredParts (ServiceId, PartId, QuantityRequired, CreatedUser)
VALUES 
(1, 3, 4, 1); -- Dịch vụ thay dầu cần 4 lít dầu Castrol (PartId = 3)

PRINT 'CarShowroomDB database v2.1 successfully created and populated with audit keys.';
GO
