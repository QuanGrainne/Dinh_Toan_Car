IF COL_LENGTH('ComboOrders', 'PurchaseType') IS NULL
BEGIN
    ALTER TABLE ComboOrders
    ADD PurchaseType NVARCHAR(20) NOT NULL CONSTRAINT DF_ComboOrders_PurchaseType DEFAULT 'Buyout';
END
GO

IF COL_LENGTH('ComboOrders', 'DepositAmount') IS NULL
BEGIN
    ALTER TABLE ComboOrders
    ADD DepositAmount DECIMAL(18,2) NULL;
END
GO

IF COL_LENGTH('ComboOrders', 'CaptchaCode') IS NULL
BEGIN
    ALTER TABLE ComboOrders
    ADD CaptchaCode NVARCHAR(20) NULL;
END
GO

IF COL_LENGTH('ComboOrders', 'CaptchaGeneratedAt') IS NULL
BEGIN
    ALTER TABLE ComboOrders
    ADD CaptchaGeneratedAt DATETIME NULL;
END
GO

IF COL_LENGTH('ComboOrders', 'IsCaptchaUsed') IS NULL
BEGIN
    ALTER TABLE ComboOrders
    ADD IsCaptchaUsed BIT NOT NULL CONSTRAINT DF_ComboOrders_IsCaptchaUsed DEFAULT 0;
END
GO

IF COL_LENGTH('ComboOrders', 'CaptchaUsedAt') IS NULL
BEGIN
    ALTER TABLE ComboOrders
    ADD CaptchaUsedAt DATETIME NULL;
END
GO

IF COL_LENGTH('ComboOrders', 'DepositExpiresAt') IS NULL
BEGIN
    ALTER TABLE ComboOrders
    ADD DepositExpiresAt DATETIME NULL;
END
GO

IF COL_LENGTH('ComboOrders', 'FinalCaptchaCode') IS NULL
BEGIN
    ALTER TABLE ComboOrders
    ADD FinalCaptchaCode NVARCHAR(20) NULL;
END
GO

IF COL_LENGTH('ComboOrders', 'FinalCaptchaGeneratedAt') IS NULL
BEGIN
    ALTER TABLE ComboOrders
    ADD FinalCaptchaGeneratedAt DATETIME NULL;
END
GO

IF COL_LENGTH('ComboOrders', 'IsFinalCaptchaUsed') IS NULL
BEGIN
    ALTER TABLE ComboOrders
    ADD IsFinalCaptchaUsed BIT NOT NULL CONSTRAINT DF_ComboOrders_IsFinalCaptchaUsed DEFAULT 0;
END
GO

IF COL_LENGTH('ComboOrders', 'FinalCaptchaUsedAt') IS NULL
BEGIN
    ALTER TABLE ComboOrders
    ADD FinalCaptchaUsedAt DATETIME NULL;
END
GO

UPDATE ComboOrders
SET PurchaseType = ISNULL(NULLIF(PurchaseType, ''), 'Buyout')
WHERE PurchaseType IS NULL OR PurchaseType = '';
GO
