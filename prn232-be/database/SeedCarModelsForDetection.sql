-- =============================================
-- SEED DATA FOR CAR MODELS IMAGE DETECTION
-- Run this script against CarShowroomDB to add car data
-- that matches predicted classes from dima806/car_models_image_detection
-- =============================================

USE CarShowroomDB;
GO

-- 1. Ensure Brands Exist
IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Toyota')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Toyota', 'Japan', 'Toyota Motor Corporation');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'BMW')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('BMW', 'Germany', 'Bayerische Motoren Werke AG');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Ford')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Ford', 'USA', 'Ford Motor Company');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Tesla')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Tesla', 'USA', 'Tesla, Inc. Electric Vehicles');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Honda')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Honda', 'Japan', 'Honda Motor Co., Ltd.');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Audi')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Audi', 'Germany', 'Audi AG - Premium Vehicles');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Mercedes-Benz')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Mercedes-Benz', 'Germany', 'Mercedes-Benz Group AG');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Porsche')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Porsche', 'Germany', 'Dr. Ing. h.c. F. Porsche AG');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Mazda')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Mazda', 'Japan', 'Mazda Motor Corporation');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Hyundai')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Hyundai', 'South Korea', 'Hyundai Motor Company');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Chevrolet')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Chevrolet', 'USA', 'General Motors - Chevrolet Division');
GO

-- 2. Insert Cars matching HuggingFace Model Classes
DECLARE @ToyotaId INT = (SELECT BrandId FROM CarBrands WHERE BrandName = 'Toyota');
DECLARE @BmwId INT = (SELECT BrandId FROM CarBrands WHERE BrandName = 'BMW');
DECLARE @FordId INT = (SELECT BrandId FROM CarBrands WHERE BrandName = 'Ford');
DECLARE @TeslaId INT = (SELECT BrandId FROM CarBrands WHERE BrandName = 'Tesla');
DECLARE @HondaId INT = (SELECT BrandId FROM CarBrands WHERE BrandName = 'Honda');
DECLARE @AudiId INT = (SELECT BrandId FROM CarBrands WHERE BrandName = 'Audi');
DECLARE @MercId INT = (SELECT BrandId FROM CarBrands WHERE BrandName = 'Mercedes-Benz');
DECLARE @PorscheId INT = (SELECT BrandId FROM CarBrands WHERE BrandName = 'Porsche');
DECLARE @MazdaId INT = (SELECT BrandId FROM CarBrands WHERE BrandName = 'Mazda');
DECLARE @HyundaiId INT = (SELECT BrandId FROM CarBrands WHERE BrandName = 'Hyundai');
DECLARE @ChevyId INT = (SELECT BrandId FROM CarBrands WHERE BrandName = 'Chevrolet');

-- Toyota Camry (Model: "Toyota Camry" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'Toyota Camry 2.5Q Hybrid')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@ToyotaId, 'Toyota Camry 2.5Q Hybrid', 'Camry', 2023, 'White', 12000, 'Hybrid', 'Automatic', 1450000000, 
            'Toyota Camry b?n Hybrid 2.5Q c?c k? ?m ?i, ti?t ki?m nhi?n li?u, trang b? g?i an to?n Toyota Safety Sense cao c?p.', 
            'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());

-- Tesla Model 3 (Model: "Tesla Model 3" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'Tesla Model 3 Long Range')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@TeslaId, 'Tesla Model 3 Long Range', 'Model 3', 2022, 'Red', 8000, 'Electric', 'Automatic', 1650000000, 
            'Xe di?n th?ng minh Tesla Model 3 b?n Long Range nh?p kh?u nguy?n chi?c. T? d?ng l?i Autopilot, n?i th?t t?i gi?n hi?n d?i.', 
            'https://images.unsplash.com/photo-1619767886558-efdc259cde1a?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());

-- Tesla Model Y (Model: "Tesla Model Y" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'Tesla Model Y Performance')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@TeslaId, 'Tesla Model Y Performance', 'Model Y', 2023, 'Black', 4000, 'Electric', 'Automatic', 1950000000, 
            'SUV di?n Tesla Model Y b?n Performance gia t?c vu?t tr?i t? 0-100 km/h ch? 3.7 gi?y. Qu?ng du?ng di chuy?n ?n tu?ng.', 
            'https://images.unsplash.com/photo-1620891549027-942fdc95d3f5?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());

-- Honda Civic (Model: "Honda Civic" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'Honda Civic 1.5 RS')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@HondaId, 'Honda Civic 1.5 RS', 'Civic', 2022, 'Red', 16000, 'Gasoline', 'Automatic', 870000000, 
            'Honda Civic 1.5L VTEC Turbo b?n RS th? thao c? t?nh, trang b? h? th?ng an to?n Honda SENSING ti?n ti?n.', 
            'https://images.unsplash.com/photo-1606016159991-dfe4f2746ad5?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());

-- Honda CR-V (Model: "Honda CR-V" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'Honda CR-V L Turbo')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@HondaId, 'Honda CR-V L Turbo', 'CR-V', 2021, 'Gray', 22000, 'Gasoline', 'Automatic', 980000000, 
            'SUV 7 ch? r?ng r?i ti?n nghi cho gia d?nh. B?o du?ng d?y d? ch?nh h?ng Honda, cam k?t kh?ng d?m d?ng ng?p nu?c.', 
            'https://images.unsplash.com/photo-1568605117036-5fe5e7bab0b7?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());

-- Ford Mustang (Model: "Ford Mustang" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'Ford Mustang Ecoboost 2.3L')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@FordId, 'Ford Mustang Ecoboost 2.3L', 'Mustang', 2020, 'Yellow', 30000, 'Gasoline', 'Automatic', 1850000000, 
            'M?u xe co b?p M? huy?n tho?i Ford Mustang d?ng co EcoBoost 2.3L m?nh m? v? ti?t ki?m. Ngo?i h?nh th? thao b?t m?t.', 
            'https://images.unsplash.com/photo-1611245801314-e0cf5bf9228d?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());

-- Audi A4 (Model: "Audi A4" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'Audi A4 40 TFSI Advance')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@AudiId, 'Audi A4 40 TFSI Advance', 'A4', 2021, 'White', 20000, 'Gasoline', 'Automatic', 1450000000, 
            'Audi A4 ki?u d?ng thanh l?ch sang tr?ng phong c?ch ch?u ?u. N?i th?t ?p g? cao c?p, h? th?ng d?n LED Matrix d?c trung.', 
            'https://images.unsplash.com/photo-1614162692292-7ac56d7f7f1e?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());

-- Mercedes-Benz C Class (Model: "Mercedes-Benz C Class" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'Mercedes-Benz C200 Avantgarde Plus')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@MercId, 'Mercedes-Benz C200 Avantgarde Plus', 'C Class', 2022, 'Black', 15000, 'Gasoline', 'Automatic', 1580000000, 
            'Mercedes-Benz C Class m?i, thi?t k? sang tr?ng th?a hu?ng t? d?ng S-Class. V?n h?nh mu?t m?, nhi?u ti?n nghi gi?i tr? th?ng minh.', 
            'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());

-- BMW 3-Series (Model: "BMW 3-Series" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'BMW 330i M Sport')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@BmwId, 'BMW 330i M Sport', '3 Series', 2022, 'Blue', 11000, 'Gasoline', 'Automatic', 1890000000, 
            'BMW 3-Series b?n 330i trang b? bodykit M Sport th? thao c? t?nh, d?ng co 258 m? l?c c?c b?c, c?m gi?c l?i d?nh cao nh?t ph?n kh?c.', 
            'https://images.unsplash.com/photo-1555215695-3004980ad54e?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());

-- Porsche 911 (Model: "Porsche 911" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'Porsche 911 Carrera S')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@PorscheId, 'Porsche 911 Carrera S', '911', 2021, 'Yellow', 6000, 'Gasoline', 'Automatic', 7800000000, 
            'Si?u xe th? thao Porsche 911 Carrera S (992) m?u v?ng Racing c?c d?p. D?ng co tang ?p k?p Boxer 3.0L, full option sang tr?ng.', 
            'https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());

-- Mazda CX-5 (Model: "Mazda CX-5" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'Mazda CX-5 2.5 Signature Premium')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@MazdaId, 'Mazda CX-5 2.5 Signature Premium', 'CX-5', 2021, 'Red', 25000, 'Gasoline', 'Automatic', 820000000, 
            'Mazda CX-5 b?n cao c?p nh?t d?ng co 2.5L d?n d?ng 2 c?u AWD, trang b? loa Bose cao c?p, m?n h?nh HUD, g?i an to?n i-Activsense.', 
            'https://images.unsplash.com/photo-1511919884226-fd3cad34687c?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());

-- Hyundai Tucson (Model: "Hyundai Tucson" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'Hyundai Tucson 1.6 Turbo')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@HyundaiId, 'Hyundai Tucson 1.6 Turbo', 'Tucson', 2022, 'White', 18000, 'Gasoline', 'Automatic', 890000000, 
            'Hyundai Tucson thi?t k? Sensuous Sportiness c?c k? tuong lai, b?n 1.6 Turbo m?nh m?, v?n h?nh ?m ?i, d?y ?p option.', 
            'https://images.unsplash.com/photo-1619767887304-4c57c46a6f1c?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());

-- Chevrolet Camaro (Model: "Chevrolet Camaro" in HF)
IF NOT EXISTS (SELECT 1 FROM Cars WHERE CarName = 'Chevrolet Camaro SS v8')
    INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
    VALUES (@ChevyId, 'Chevrolet Camaro SS v8', 'Camaro', 2019, 'Yellow', 29000, 'Gasoline', 'Automatic', 2600000000, 
            'Xe th? thao co b?p Chevrolet Camaro b?n SS d?ng co V8 6.2L c?c kh?ng, ?m thanh p? g?m r? m?nh m?, m?u v?ng Bumblebee.', 
            'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?auto=format&fit=crop&w=600&q=80', 'Available', GETDATE());
GO

PRINT 'Car Model seeds completed successfully!';
