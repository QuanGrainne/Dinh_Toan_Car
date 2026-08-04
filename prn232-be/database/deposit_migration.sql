-- =============================================
-- DEPOSIT FLOW MIGRATION
-- Run this script once against CarShowroomDB
-- =============================================

-- 1. Extend PurchaseRequests with deposit fields
ALTER TABLE PurchaseRequests
    ADD DepositAmount   DECIMAL(18,2)  NULL,
        DepositDate     DATETIME       NULL,
        DepositExpiry   DATETIME       NULL,
        CaptchaCode     NVARCHAR(20)   NULL;

-- 2. Allow 'Deposited' status
ALTER TABLE PurchaseRequests
    DROP CONSTRAINT IF EXISTS CK__PurchaseR__Statu__XX;   -- drop old CHECK if named

-- Re-add without constraint name (EF ignores named checks on update)
-- The application-level validation covers allowed statuses.

-- 3. DepositCaptchas - admin inserts codes here; user consumes one to lock a deposit
CREATE TABLE DepositCaptchas (
    CaptchaId   INT IDENTITY(1,1) PRIMARY KEY,
    Code        NVARCHAR(20) NOT NULL UNIQUE,
    CarId       INT NOT NULL,           -- optional: tie captcha to a specific car
    IsUsed      BIT NOT NULL DEFAULT 0,
    CreatedAt   DATETIME NOT NULL DEFAULT GETDATE(),
    UsedAt      DATETIME NULL,
    CONSTRAINT FK_DepositCaptchas_Cars FOREIGN KEY (CarId) REFERENCES Cars(CarId)
);

-- Sample captcha seeds (admin would insert these via admin panel later)
-- INSERT INTO DepositCaptchas (Code, CarId) VALUES ('ABC123', 1), ('XYZ789', 1);
