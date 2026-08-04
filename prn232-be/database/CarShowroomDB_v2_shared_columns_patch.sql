-- =========================================================================
-- PATCH: Shared invoice/payment columns required by application code
-- Áp dụng SAU khi chạy CarShowroomDB_v2.sql
-- =========================================================================
-- Lý do: Bảng MasterInvoices trong v2 là hóa đơn "tổng" dùng chung cho cả 3 module
--        (ô tô, phụ tùng, dịch vụ). Code hiện tại của cả 3 module cần thêm một số
--        cột vận hành mà schema v2 gốc chưa có. Patch này BỔ SUNG (additive, không
--        phá dữ liệu) các cột đó để toàn hệ thống chạy đúng.
--
--   * InvoiceType, PaidAt          -> module Ô TÔ dùng để phân loại & đánh dấu đã thanh toán
--   * PaymentMethod, PaymentReference, PaidAt -> module PHỤ TÙNG / DỊCH VỤ dùng
--   * MaintenanceAppointments.IsPaid          -> module DỊCH VỤ dùng
-- =========================================================================

USE CarShowroomDB;
GO

-- ---- MasterInvoices: cột phân loại & thanh toán dùng chung ----
IF COL_LENGTH('MasterInvoices', 'InvoiceType') IS NULL
    ALTER TABLE MasterInvoices ADD InvoiceType VARCHAR(20) NOT NULL
        CONSTRAINT DF_MasterInvoices_InvoiceType DEFAULT 'Car';
GO

IF COL_LENGTH('MasterInvoices', 'PaymentMethod') IS NULL
    ALTER TABLE MasterInvoices ADD PaymentMethod NVARCHAR(50) NULL;
GO

IF COL_LENGTH('MasterInvoices', 'PaymentReference') IS NULL
    ALTER TABLE MasterInvoices ADD PaymentReference NVARCHAR(100) NULL;
GO

IF COL_LENGTH('MasterInvoices', 'PaidAt') IS NULL
    ALTER TABLE MasterInvoices ADD PaidAt DATETIME NULL;
GO

-- Index hỗ trợ lọc hóa đơn theo module (ô tô truy vấn InvoiceType = 'Car')
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MasterInvoices_InvoiceType')
    CREATE INDEX IX_MasterInvoices_InvoiceType ON MasterInvoices(InvoiceType);
GO

-- ---- MaintenanceAppointments: cờ đã thanh toán (module dịch vụ) ----
IF COL_LENGTH('MaintenanceAppointments', 'IsPaid') IS NULL
    ALTER TABLE MaintenanceAppointments ADD IsPaid BIT NOT NULL
        CONSTRAINT DF_MaintenanceAppointments_IsPaid DEFAULT 0;
GO

PRINT 'Shared columns patch applied successfully.';
GO
